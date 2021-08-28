using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BattleStateMachine : MonoBehaviour
{
    private GameManager GM;
    private EventManager EM;
    // VARIABLES
    public List<Transform> _HeroSpawns;    // Where do they initially spawn?
    public List<Transform> _EnemySpawns;

    private static List<GameObject> _HeroModels = new List<GameObject>();  // The character models in scene
    private static List<GameObject> _EnemyModels = new List<GameObject>();

    public static List<HeroExtension> _HeroesActive = new List<HeroExtension>(); // Who are currently in battle and can take actions
    public static List<EnemyExtension> _EnemiesActive = new List<EnemyExtension>();

    public static List<HeroExtension> _HeroesDowned = new List<HeroExtension>(); // Who are currently in battle but are K.O'd
    public static List<EnemyExtension> _EnemiesDowned = new List<EnemyExtension>();

    public BattleState _BattleState;
    private bool _EndBattleLock;

    // UPDATES
    private void Awake()
    {
        GM = FindObjectOfType<GameManager>();
        EM = FindObjectOfType<EventManager>();
    }
    private void Start()
    {
        _EndBattleLock = false;
        StartCoroutine(BattleIntermission(5));
        SpawnCharacterModels();
        Debug.Log("Enemies active:"+_EnemiesActive.Count);
        Debug.Log("Enemies Downed:" + _EnemiesDowned.Count);
    }
    private void Update()
    {
        if(!_EndBattleLock)
        {
            switch (_BattleState)
            {
                case BattleState.ACTIVE:
                    EndBattleCondition();
                    BattleActiveState();
                    break;

                case BattleState.WAIT:
                    EndBattleCondition();
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
        for (int i = 0; i < GM._PartyLineup.Count; i++)
        {
            GameObject instantiatedHero = Instantiate(GM._PartyLineup[i]._CharacterModel,
                                                        _HeroSpawns[i].position,
                                                        _HeroSpawns[i].rotation,
                                                        GameObject.Find("Hero Model Data").transform);
            instantiatedHero.name = GM._PartyLineup[i].charName +" Model";
            _HeroModels.Add(instantiatedHero);

            _HeroesActive.Add(GM._PartyLineup[i]);
            GM._PartyLineup[i]._MyInstantiatedModel = instantiatedHero;
        }
    }
    private void AddEnemyIntoBattle()
    {
        for (int i = 0; i < GM._EnemyLineup.Count; i++)
        {
            // Instantiate enemy models
            GameObject instantiatedEnemyModel = Instantiate(GM._EnemyLineup[i]._CharacterModel,
                                                            _EnemySpawns[i].position,
                                                            _EnemySpawns[i].rotation,
                                                            GameObject.Find("Enemy Model Data").transform);
            _EnemyModels.Add(instantiatedEnemyModel);
            instantiatedEnemyModel.name = GM._EnemyLineup[i].charName + " Model " + i;

            // Instantiate a new version of enemy script
            EnemyExtension instantiatedEnemyClass = Instantiate(GM._EnemyLineup[i], 
                                                                GameObject.Find("Enemy Battle Data").transform);
            instantiatedEnemyClass.name = GM._EnemyLineup[i].charName + " Data " + i;
            _EnemiesActive.Add(instantiatedEnemyClass);
            instantiatedEnemyClass._MyInstantiatedModel = instantiatedEnemyModel;
        }
    }
    #endregion
    #region CHANGE IN BATTLE STATE
    private void BattleActiveState()
    {
        foreach(CharacterCircuit cc in _HeroesActive)
        {
            cc.ActiveStateBehaviour();
        }
        foreach(CharacterCircuit cx in _EnemiesActive)
        {
            cx.ActiveStateBehaviour();
        }
    }
    private IEnumerator BattleIntermission(float x)
    {
        print("START INTERMISSION");
        _BattleState = BattleState.WAIT;
        yield return new WaitForSeconds(x);
        _BattleState = BattleState.ACTIVE;
        print("END INTERMISSION " +"("+ x +") seconds");
    }
    #endregion
    #region CHECK STATE OF BATTLERS
    // DEATH CHECK. Called when a char is hit
    public void CheckCharIsDead(HeroExtension hero)
    {
        _HeroesActive.Remove(hero);
        _HeroesDowned.Add(hero);
        hero._MyInstantiatedModel.SetActive(false);
        Debug.Log(hero.charName + " has fallen!");
        Debug.Log(_HeroesActive + " Heroes remaining");
    }
    public void CheckCharIsDead(EnemyExtension enemy)
    {
        _EnemiesActive.Remove(enemy);
        _EnemiesDowned.Add(enemy);
        enemy._MyInstantiatedModel.SetActive(false);
        Debug.Log(enemy.charName + " has fallen!");
        Debug.Log("Enemies still Active: " + _EnemiesActive.Count);
        Debug.Log("Enemies K.O'd: " + _EnemiesDowned.Count);
    }
    #endregion
    #region END OF GAME
    private void EndBattleCondition()
    {
        if (_HeroesActive.Count > 0 && _EnemiesActive.Count <= 0)
        {
            _BattleState = BattleState.WAIT;
            StartCoroutine(VictoryTransition());
        }
        else if (_EnemiesActive.Count > 0 && _HeroesActive.Count <= 0)
        {
            _BattleState = BattleState.WAIT;
            StartCoroutine(DefeatTransition());
        }
    }
    private IEnumerator VictoryTransition()
    {
        // Victory poses, exp gaining, items, transition back to overworld
        _EndBattleLock = true;
        Debug.Log("VICTORY!!!");
        DistributeTheExp();
        ReturnToOverworldPrep();
        yield return null;
    }
    private void DistributeTheExp()
    {
        int sharedExp = 0;
        foreach(EnemyExtension enemy in _EnemiesDowned)
        {
            sharedExp += enemy.experiencePool;
        }
        foreach(HeroExtension hero in _HeroesActive)
        {
            hero._TotalExperience += (int)(sharedExp / _HeroesActive.Count);
            Debug.Log(hero.charName + " has gained " + (int)(sharedExp / _HeroesActive.Count) + " EXP!!!");
            hero.LevelUpCheck();
        }
    }
    private void ReturnToOverworldPrep()
    {
        // Clear Data
        _HeroModels.Clear();
        _HeroesActive.Clear();
        _HeroesDowned.Clear();

        _EnemyModels.Clear();
        _EnemiesActive.Clear();
        _EnemiesDowned.Clear();

        GM._EnemyLineup.Clear();

        Destroy(GameObject.Find("Enemy Data"));
        // reload scene and create player moving character at coordinates
        EM.ChangeFunction(GameState.OVERWORLD);
    }
    private IEnumerator DefeatTransition()
    {
        // Lost, Open up UI options to load saved game or return to title
        _EndBattleLock = true;
        Debug.Log("GAME OVER!!!");
        yield return null;
    }
    #endregion
}