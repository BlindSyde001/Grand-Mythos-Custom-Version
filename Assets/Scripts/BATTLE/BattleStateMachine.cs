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

    private List<GameObject> _HeroModels;  // The character models in scene
    private List<GameObject> _EnemyModels;

    public List<HeroExtension> _HeroesActive; // Who are currently in battle and can take actions
    public List<EnemyExtension> _EnemiesActive;

    public List<HeroExtension> _HeroesDowned; // Who are currently in battle but are K.O'd
    public List<EnemyExtension> _EnemiesDowned;

    public BattleState _BattleState;

    // UPDATES
    private void Awake()
    {
        GM = FindObjectOfType<GameManager>();

        _HeroModels = new List<GameObject>();
        _EnemyModels = new List<GameObject>();
        _HeroesActive = new List<HeroExtension>();
        _EnemiesActive = new List<EnemyExtension>();
        _HeroesDowned = new List<HeroExtension>();
        _EnemiesDowned = new List<EnemyExtension>();
    }
    private void Start()
    {
        StartCoroutine(BattleIntermission(5));
        SpawnCharacterModels();
    }
    private void Update()
    {
        EndBattleCondition();
        switch (_BattleState)
        {
            case BattleState.ACTIVE:
                BattleActiveState();
                break;

            case BattleState.WAIT:
                break;
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
        Debug.Log(enemy.charName + " has fallen!");
    }
    #endregion
    #region END OF GAME
    private IEnumerator EndOfBattleTransition()
    {
        // Victory poses, exp gaining, items, transition back to overworld
        yield return null;
    }
    private void EndBattleCondition()
    {
        if (_HeroesActive.Count == 0 && _HeroesDowned.Count > 0)
        {
            return;
        }
        else if (_EnemiesActive.Count == 0 && _EnemiesDowned.Count > 0)
        {
            return;
        }
    }
    #endregion
}
