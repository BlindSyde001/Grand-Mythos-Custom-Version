using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BattleStateMachine : MonoBehaviour
{
    private GameManager GM;
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
    }
    private void Start()
    {
        _EndBattleLock = false;
        StartCoroutine(BattleIntermission(5));
        SpawnCharacterModels();
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
                _HeroSpawns[i].rotation);
            instantiatedHero.name = GM._PartyLineup[i].charName +" Model";
            _HeroModels.Add(instantiatedHero);

            _HeroesActive.Add(GM._PartyLineup[i]);
        }
    }
    private void AddEnemyIntoBattle()
    {
        for (int i = 0; i < GM._EnemyLineup.Count; i++)
        {
            GameObject instantiatedEnemy = Instantiate(GM._EnemyLineup[i]._CharacterModel as GameObject,
                _EnemySpawns[i].position,
                _EnemySpawns[i].rotation);
            _EnemyModels.Add(instantiatedEnemy);
            instantiatedEnemy.name = GM._EnemyLineup[i].charName + " Model " + i;

            _EnemiesActive.Add(GM._EnemyLineup[i]);
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
        Debug.Log(hero.charName + " has fallen!");
        Debug.Log(_HeroesActive + "Remaining");
    }
    public void CheckCharIsDead(EnemyExtension enemy)
    {
        _EnemiesActive.Remove(enemy);
        _EnemiesDowned.Add(enemy);
        Debug.Log(_EnemiesActive.Count);
        Debug.Log(_EnemiesDowned.Count);
        Debug.Log(enemy.charName + " has fallen!");
    }
    #endregion
    #region END OF GAME
    private void EndBattleCondition()
    {
        if (_HeroesActive.Count == 0 && _HeroesDowned.Count > 0)
        {
            _BattleState = BattleState.WAIT;
            StartCoroutine(VictoryTransition());
        }
        else if (_EnemiesActive.Count == 0 && _EnemiesDowned.Count > 0)
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
            hero._Experience += (int)(sharedExp / _HeroesActive.Count);
        }
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
