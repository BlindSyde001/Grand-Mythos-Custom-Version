using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class BattleStateMachine : MonoBehaviour
{
    private BattleUIController BU;
    private GameManager gameManager;

    // VARIABLES
    public List<Transform> _HeroSpawns;    // Where do they initially spawn?
    public List<Transform> _EnemySpawns;

    private static List<GameObject> _HeroModels = new();  // The character models in scene
    private static List<GameObject> _EnemyModels = new();

    public static List<EnemyExtension> _EnemiesActive = new();
    public static List<EnemyExtension> _EnemiesDowned = new();

    public CombatState _BattleState;
    public GameObject losePanel;
    private bool _EndBattleLock;

    private CinemachineFreeLook rotateCam;

    // UPDATES
    private void Awake()
    {
        BU = FindObjectOfType<BattleUIController>();
        rotateCam = FindObjectOfType<CinemachineFreeLook>();
    }
    private void Start()
    {
        gameManager = GameManager._instance;
        _EndBattleLock = false;
        SpawnCharacterModels();
        StartCoroutine(BattleIntermission(5));
    }
    private void Update()
    {
        if(!_EndBattleLock)
        {
            switch (_BattleState)
            {
                case CombatState.ACTIVE:
                    EndBattleCondition();
                    BattleActiveState();
                    rotateCam.GetComponent<CinemachineFreeLook>().enabled = true;
                    break;

                case CombatState.WAIT:
                    EndBattleCondition();
                    rotateCam.GetComponent<CinemachineFreeLook>().enabled = false;
                    break;
            }
        }
    }

    // METHODS
    #region START OF BATTLE
    private void SpawnCharacterModels() // Spawn models into game, add heroes into active or downed list for battle.
    {
        AddHeroIntoBattle();
        AddEnemyIntoBattle();
    }
    private void AddHeroIntoBattle()
    {
        for (int i = 0; i < gameManager._PartyLineup.Count; i++)
        {
            #region Instantiate Hero Model
            GameObject instantiatedHero = Instantiate(gameManager._PartyLineup[i]._CharacterModel,
                                                        _HeroSpawns[i].position,
                                                        _HeroSpawns[i].rotation,
                                                        GameObject.Find("Hero Model Data").transform);
            instantiatedHero.name = gameManager._PartyLineup[i].charName + " Model";
            _HeroModels.Add(instantiatedHero);
            #endregion
            #region Add to Active/Downed List based on Health
            switch (gameManager._PartyLineup[i]._CurrentHP)
            {
                case <= 0:
                    gameManager._PartyMembersDowned.Add(gameManager._PartyLineup[i]);
                    break;

                case > 0:
                    gameManager._PartyMembersActive.Add(gameManager._PartyLineup[i]);
                    break;
            }
            gameManager._PartyLineup[i]._MyInstantiatedModel = instantiatedHero;
            #endregion
            gameManager._PartyLineup[i].myBattleHeroController.anim = instantiatedHero.GetComponent<Animator>();
            BU.CreateHeroUI(gameManager._PartyLineup[i]);
        }
        foreach (HeroExtension a in gameManager._PartyMembersActive)
        {
            a._ActionChargeAmount = Random.Range(0, 50);
        }
    }
    private void AddEnemyIntoBattle()
    {
        for (int i = 0; i < gameManager._EnemyLineup.Count; i++)
        {
            #region Instantiate Enemy Model
            GameObject instantiatedEnemyModel = Instantiate(gameManager._EnemyLineup[i]._CharacterModel,
                                                            _EnemySpawns[i].position,
                                                            _EnemySpawns[i].rotation,
                                                            GameObject.Find("Enemy Model Data").transform);
            _EnemyModels.Add(instantiatedEnemyModel);
            instantiatedEnemyModel.name = gameManager._EnemyLineup[i].charName + " Model " + i;
            #endregion
            #region Create new Instance of the enemy Script Class
            EnemyExtension instantiatedEnemyClass = Instantiate(gameManager._EnemyLineup[i], 
                                                                GameObject.Find("Enemy Battle Data").transform);
            instantiatedEnemyClass.name = gameManager._EnemyLineup[i].charName + " Data " + i;
            instantiatedEnemyClass._MyInstantiatedModel = instantiatedEnemyModel;
            _EnemiesActive.Add(instantiatedEnemyClass);
            #endregion
            gameManager._EnemyLineup[i].myBattleEnemyController.anim = instantiatedEnemyModel.GetComponent<Animator>();
            BU.CreateEnemyUI(instantiatedEnemyClass, instantiatedEnemyModel.transform);

            gameManager._EnemyLineup[i]._ActionChargeAmount = Random.Range(0, 50);
        }
    }
    #endregion
    #region CHANGE IN BATTLE STATE
    private void BattleActiveState()
    {
        foreach(HeroExtension hero in gameManager._PartyMembersActive)
        {
            hero.myBattleHeroController.ActiveStateBehaviour();
        }
        foreach(EnemyExtension enemy in _EnemiesActive)
        {
            enemy.myBattleEnemyController.ActiveStateBehaviour();
        }
    }
    private IEnumerator BattleIntermission(float x)
    {
        _BattleState = CombatState.WAIT;
        yield return new WaitForSeconds(x);
        _BattleState = CombatState.ACTIVE;
    }
    #endregion
    #region CHECK STATE OF BATTLERS
    // DEATH CHECK. Called when a char is hit
    public void CheckCharIsDead(HeroExtension hero)
    {
        gameManager._PartyMembersActive.Remove(hero);
        gameManager._PartyMembersDowned.Add(hero);
        hero._MyInstantiatedModel.SetActive(false);
        Debug.Log(hero.charName + " has fallen!");
    }
    public void CheckCharIsDead(EnemyExtension enemy)
    {
        _EnemiesActive.Remove(enemy);
        _EnemiesDowned.Add(enemy);
        enemy._MyInstantiatedModel.SetActive(false);
        Debug.Log(enemy.charName + " has fallen!");
    }
    #endregion
    #region END OF GAME
    private void EndBattleCondition()
    {
        if (gameManager._PartyMembersActive.Count > 0 && _EnemiesActive.Count <= 0)
        {
            _BattleState = CombatState.WAIT;
            StartCoroutine(VictoryTransition());
        }
        else if (_EnemiesActive.Count > 0 && gameManager._PartyMembersActive.Count <= 0)
        {
            _BattleState = CombatState.WAIT;
            StartCoroutine(DefeatTransition());
        }
    }

    private IEnumerator VictoryTransition()
    {
        // Victory poses, exp gaining, items, transition back to overworld
        _EndBattleLock = true;
        Debug.Log("VICTORY!!!");
        DistributeTheRewards();
        yield return null;
    }
    private void DistributeTheRewards()
    {
        int sharedExp = 0;
        int creditsEarned = 0;
        foreach(EnemyExtension enemy in _EnemiesDowned)
        {
            sharedExp += enemy.experiencePool;
            creditsEarned += enemy.creditPool;
        }
        foreach(HeroExtension hero in gameManager._PartyMembersActive)
        {
            hero._TotalExperience += sharedExp / gameManager._PartyMembersActive.Count;
            Debug.Log(hero.charName + " has gained " + sharedExp / gameManager._PartyMembersActive.Count + " EXP & " + creditsEarned + " Credits!!!" );
            hero.LevelUpCheck();
        }
        InventoryManager._instance.creditsInBag += creditsEarned;
        ReturnToOverworldPrep();
    }
    private void ReturnToOverworldPrep()
    {
        ClearData();

        // reload scene and create player moving character at coordinates
        EventManager._instance.SwitchNewScene(2);
    }

    private IEnumerator DefeatTransition()
    {
        // Lost, Open up UI options to load saved game or return to title
        _EndBattleLock = true;
        Debug.Log("GAME OVER!!!");
        OpenLoseMenu();
        yield return null;
    }
    private void OpenLoseMenu()
    {
        losePanel.SetActive(true);
    }
    #endregion

    public void ClearData()
    {
        _HeroModels.Clear();
        gameManager._PartyMembersActive.Clear();
        gameManager._PartyMembersDowned.Clear();

        _EnemyModels.Clear();
        _EnemiesActive.Clear();
        _EnemiesDowned.Clear();

        gameManager._EnemyLineup.Clear();

        Destroy(GameObject.Find("Enemy Data"));
    }
}
