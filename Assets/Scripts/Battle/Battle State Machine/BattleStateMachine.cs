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

    #region Controller Variables
    public static List<BattleHeroController> _HeroControllers = new(); // Where all actions for Battle are
    public static List<BattleEnemyController> _EnemyControllers = new();
    #endregion
    #region Model Variables
    private static List<GameObject> _HeroModels = new();  // The character models in scene
    private static List<GameObject> _EnemyModels = new();
    #endregion
    #region Character State Variables
    public static List<BattleHeroController> _HeroesActive = new();
    public static List<BattleHeroController> _HeroesDowned = new();

    public static List<BattleEnemyController> _EnemiesActive = new();
    public static List<BattleEnemyController> _EnemiesDowned = new();
    #endregion

    public static CombatState _CombatState;
    public GameObject losePanel;
    private bool _EndBattleLock;

    private CinemachineFreeLook rotateCam;

    // UPDATES
    private void Awake()
    {
        BU = FindObjectOfType<BattleUIController>();
        rotateCam = FindObjectOfType<CinemachineFreeLook>();
        //Cursor.visible = false;
    }
    private void Start()
    {
        gameManager = GameManager._instance;
        _EndBattleLock = false;
        SpawnCharacterModels();
        SwitchCombatState(CombatState.START);
        StartCoroutine(BattleIntermission(5));
    }
    private void Update()
    {
        if(!_EndBattleLock)
        {
            switch (_CombatState)
            {
                case CombatState.ACTIVE:
                    EndBattleCondition();
                    BattleIsActive();
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
    private void SwitchCombatState(CombatState newCombatState)
    {
        switch(newCombatState)
        {
            case CombatState.START:
                _CombatState = CombatState.START;
                break;

            case CombatState.ACTIVE:
                _CombatState = CombatState.ACTIVE;
                break;

            case CombatState.WAIT:
                _CombatState = CombatState.WAIT;
                break;

            case CombatState.END:
                _CombatState = CombatState.END;
                break;
        }
    }

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
            // Add the Controller
             _HeroControllers.Add(gameManager._PartyLineup[i].myBattleHeroController);

            // Add Model into Battle
            GameObject instantiatedHeroModel = Instantiate(_HeroControllers[i].myHero._CharacterModel, 
                                                      _HeroSpawns[i].position, 
                                                      _HeroSpawns[i].rotation, 
                                                      GameObject.Find("Hero Model Data").transform);
            instantiatedHeroModel.name = _HeroControllers[i].myHero.charName + " Model";
            _HeroModels.Add(instantiatedHeroModel);

            // Add to Active/Downed list Based on Health
            switch (_HeroControllers[i].myHero._CurrentHP)
            {
                case <= 0:
                    _HeroesDowned.Add(_HeroControllers[i]);
                    break;

                case > 0:
                    _HeroesActive.Add(_HeroControllers[i]);
                    break;
            }

            // Attach Relevant References
            _HeroControllers[i].myHero._MyInstantiatedModel = instantiatedHeroModel;        // The Battle Model Im using
            _HeroControllers[i].anim = instantiatedHeroModel.GetComponent<Animator>();      // The Animator Component
            BU.CreateHeroUI(_HeroControllers[i].myHero);                                    // Battle UI Component
        }
        foreach (BattleHeroController a in _HeroesActive)
        {
            a.myHero._ActionChargeAmount = Random.Range(0, 50);
        }                                // Set ATB Bar
    }
    private void AddEnemyIntoBattle()
    {
        for (int i = 0; i < gameManager._EnemyLineup.Count; i++)
        {
            // Add Controller
            _EnemyControllers.Add(gameManager._EnemyLineup[i].myBattleEnemyController);

            // Add Model into Battle
            GameObject instantiatedEnemyModel = Instantiate(_EnemyControllers[i].myEnemy._CharacterModel,
                                                            _EnemySpawns[i].position,
                                                            _EnemySpawns[i].rotation,
                                                            GameObject.Find("Enemy Model Data").transform);

            instantiatedEnemyModel.name = _EnemyControllers[i].myEnemy.charName + " Model " + i;
            _EnemyModels.Add(instantiatedEnemyModel);

            // Add to Active List
            _EnemiesActive.Add(gameManager._EnemyLineup[i].myBattleEnemyController);

            // Attach Relevant References
            gameManager._EnemyLineup[i]._MyInstantiatedModel = instantiatedEnemyModel;               // the Model Im using in Battle
            _EnemyControllers[i].anim = instantiatedEnemyModel.GetComponent<Animator>();             // The Animator Component
            BU.CreateEnemyUI(gameManager._EnemyLineup[i], instantiatedEnemyModel.transform);         // the Battle UI  Component

            _EnemyControllers[i].myEnemy._ActionChargeAmount = Random.Range(0, 50);                  // The ATB Bar
        }
    }
    #endregion
    #region CHANGE IN BATTLE STATE
    private void BattleIsActive()
    {
        foreach(BattleHeroController hero in _HeroControllers)
        {
            hero.ActiveStateBehaviour();
        }
        foreach(BattleEnemyController enemy in _EnemyControllers)
        {
            enemy.ActiveStateBehaviour();
        }
    }
    private IEnumerator BattleIntermission(float x)
    {
        //_CombatState = CombatState.START;
        SwitchCombatState(CombatState.WAIT);
        yield return new WaitForSeconds(x);
        SwitchCombatState(CombatState.ACTIVE);
    }
    #endregion
    #region CHECK STATE OF BATTLERS
    // DEATH CHECK. Called when a char is hit
    public void CheckCharIsDead(BattleHeroController hero)
    {
        _HeroesActive.Remove(hero);
        _HeroesDowned.Add(hero);
        hero.myHero._MyInstantiatedModel.SetActive(false);
        Debug.Log(hero.myHero.charName + " has fallen!");
    }
    public void CheckCharIsDead(BattleEnemyController enemy)
    {
        _EnemiesActive.Remove(enemy);
        _EnemiesDowned.Add(enemy);
        enemy.myEnemy._MyInstantiatedModel.SetActive(false);
        Debug.Log(enemy.myEnemy.charName + " has fallen!");
    }
    #endregion
    #region End of Battle
    private void EndBattleCondition()
    {
        if (_HeroesActive.Count > 0 && _EnemiesActive.Count <= 0)
        {
            SwitchCombatState(CombatState.WAIT);
            StartCoroutine(VictoryTransition());
        }
        else if (_EnemiesActive.Count > 0 && _HeroesActive.Count <= 0)
        {
            SwitchCombatState(CombatState.WAIT);
            StartCoroutine(DefeatTransition());
        }
    }
    #region Win Fight
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
        foreach(BattleEnemyController enemy in _EnemiesDowned)
        {
            sharedExp += enemy.myEnemy.experiencePool;
            creditsEarned += enemy.myEnemy.creditPool;
        }
        foreach(BattleHeroController hero in _HeroesActive)
        {
            hero.myHero._TotalExperience += sharedExp / _HeroesActive.Count;
            hero.myHero.LevelUpCheck();
        }
        InventoryManager._instance.creditsInBag += creditsEarned;
        StartCoroutine(ReturnToOverworldPrep());
    }
    private IEnumerator ReturnToOverworldPrep()
    {
        ClearData();
        yield return new WaitForSeconds(2);
        Cursor.visible = true;
        // reload scene and create player moving character at coordinates
        EventManager._instance.SwitchNewScene(2);
    }
    #endregion
    #region Lost fight
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
        _HeroesActive.Clear();
        _HeroesDowned.Clear();
        _HeroControllers.Clear();

        _EnemyModels.Clear();
        _EnemiesActive.Clear();
        _EnemiesDowned.Clear();
        _EnemyControllers.Clear();

        foreach(EnemyExtension ext in gameManager._EnemyLineup)
        {
            Destroy(ext.gameObject);
        }
        gameManager._EnemyLineup.Clear();
    }
    #endregion
}