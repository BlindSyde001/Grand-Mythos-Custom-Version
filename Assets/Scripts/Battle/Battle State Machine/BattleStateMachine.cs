using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Random = UnityEngine.Random;

public class BattleStateMachine : MonoBehaviour
{
    public Team PlayerTeam;
    public SerializableHashSet<CharacterTemplate> Units = new();
    public SerializableHashSet<CharacterTemplate> TacticsDisabled = new();

    [NonSerialized]
    public BlockBattleFlags blocked;

    List<CharacterTemplate> _unitsCopy = new();
    Dictionary<CharacterTemplate, Tactics> _orders = new();
    [SerializeField] BattleResolution battleResolution;

    // VARIABLES
    public List<Transform> _HeroSpawns;    // Where do they initially spawn?
    public List<Transform> _EnemySpawns;

    #region Controller Variables
    List<BattleHeroModelController> _HeroControllers = new(); // Where all actions for Battle are
    List<BattleEnemyModelController> _EnemyControllers = new();
    #endregion
    public delegate void SwitchToNewState(CombatState CS);
    public static event SwitchToNewState OnNewStateSwitched;

    CombatState _combatState;
    CinemachineFreeLook rotateCam;

    // UPDATES
    void Awake()
    {
        rotateCam = FindObjectOfType<CinemachineFreeLook>();
    }

    void Start()
    {
        var gameManager = GameManager._instance;
        for (int i = 0; i < gameManager._ReservesLineup.Count; i++)
            gameManager._ReservesLineup[i].gameObject.SetActive(false);

        for (int i = 0; i < gameManager._PartyLineup.Count; i++)
        {
            var hero = gameManager._PartyLineup[i];

            // Add Model into Battle
            var model = Instantiate(hero._CharacterModel,
                _HeroSpawns[i].position,
                _HeroSpawns[i].rotation,
                GameObject.Find("Hero Model Data").transform);

            if (model.TryGetComponent(out BattleHeroModelController controller) == false)
                controller = model.AddComponent<BattleHeroModelController>();
            _HeroControllers.Add(controller);

            // Attach Relevant References
            controller.animator = model.GetComponent<Animator>();  // The Animator Component
            controller.myMovementController = model.GetComponent<BattleArenaMovement>();
            controller.myHero = hero;
            model.name = $"{hero.charName} Model";

            gameManager._PartyLineup[i].ActionChargePercent = Random.Range(0, 50);
        }

        for (int i = 0; i < gameManager._EnemyLineup.Count; i++)
        {
            var enemy = gameManager._EnemyLineup[i];

            // Add Model into Battle
            var model = Instantiate(enemy._CharacterModel,
                _EnemySpawns[i].position,
                _EnemySpawns[i].rotation,
                enemy.transform);

            if (model.TryGetComponent(out BattleEnemyModelController controller) == false)
                controller = model.AddComponent<BattleEnemyModelController>();
            _EnemyControllers.Add(controller);

            model.name = $"{enemy.charName} Model {i}";

            // Attach Relevant References
            controller.animator = model.GetComponent<Animator>();         // The Animator Component
            controller.myEnemy = enemy;
            enemy.ActionChargePercent = Random.Range(0, 50);                  // The ATB Bar
        }

        SwitchCombatState(CombatState.START);
        StartCoroutine(BattleIntermission(5, CombatState.START));
    }

    void OnEnable()
    {
        #warning this is not flexible enough, what if a character comes in in the middle of a fight
        foreach (CharacterTemplate target in FindObjectsOfType<CharacterTemplate>())
        {
            if (target.isActiveAndEnabled)
                Units.Add(target);
        }
    }

    void CleanUpdate()
    {
        if (blocked != 0)
            return;

        int alliesLeft = 0;
        int hostilesLeft = 0;
        foreach (var target in Units)
        {
            if (target._CurrentHP == 0)
                continue;
            if (PlayerTeam.Allies.Contains(target.Team))
                alliesLeft++;
            else
                hostilesLeft++;
        }

        if (hostilesLeft == 0)
        {
            rotateCam.GetComponent<CinemachineFreeLook>().enabled = false;
            SwitchCombatState(CombatState.END);
            enabled = false;
            StartCoroutine(VictoryTransition());
        }
        else if (alliesLeft == 0)
        {
            rotateCam.GetComponent<CinemachineFreeLook>().enabled = false;
            SwitchCombatState(CombatState.END);
            enabled = false;
            StartCoroutine(DefeatTransition());
        }

        foreach (var unit in Units)
        {
            if (unit._CurrentHP != 0)
                unit.ActionChargePercent += unit.ActionRechargeSpeed * Time.deltaTime;
            // DO NOT CLAMP THE CHARGE, WE SHOULDN'T REMOVE ANY CHARGE THE UNIT GAINED
        }

        _unitsCopy.Clear();
        _unitsCopy.AddRange(Units);
        foreach (var unit in Units)
        {
            if (unit._CurrentHP == 0 || TacticsDisabled.Contains(unit))
                continue;

            if (unit.ActionChargePercent < 100)
                continue;

            blocked |= BlockBattleFlags.UnitAct;
            StartCoroutine(ProcessUnit(unit));
            return;
        }
    }


    IEnumerator ProcessUnit(CharacterTemplate unit)
    {
        blocked |= BlockBattleFlags.UnitAct;
        #if UNITY_EDITOR
        // Halting execution to reload assemblies while this enumerator is running
        // may put the state of the combat into an unrecoverable state, make sure that doesn't happen
        UnityEditor.EditorApplication.LockReloadAssemblies();
        #endif
        try
        {
            unit.Context.Source = unit;
            bool foundTactics = false;
            Tactics chosenTactic;
            TargetCollection selection = default;
            if (_orders.TryGetValue(unit, out chosenTactic) && chosenTactic.Condition.CanExecuteWithAction(chosenTactic.Actions, new TargetCollection(_unitsCopy), unit.Context, out selection))
            {
                // We're taking care of this explicit order
            }
            else
            {
                foreach (var tactic in unit.Tactics)
                {
                    if (tactic.IsOn && tactic.Condition.CanExecuteWithAction(tactic.Actions, new TargetCollection(_unitsCopy), unit.Context, out selection))
                    {
                        chosenTactic = tactic;
                        break;
                    }
                }
            }
            _orders.Remove(unit);

            if (chosenTactic != null)
            {
                foreach (var action in chosenTactic.Actions)
                {
                    float percentCost = action.ATBCost * 100f / unit.ActionChargeMax;
                    if (unit.ActionChargePercent < percentCost)
                        break;

                    foundTactics = true;
                    unit.ActionChargePercent -= percentCost;

                    foreach (var yield in action.Perform(selection, unit.Context))
                        yield return yield;

                    if (Units.Contains(unit) == false)
                        break;
                }

                chosenTactic.Condition.TargetFilter?.NotifyUsedCondition(selection, unit.Context);
                chosenTactic.Condition.AdditionalCondition?.NotifyUsedCondition(selection, unit.Context);
                foreach (var action in chosenTactic.Actions)
                {
                    action.TargetFilter?.NotifyUsedCondition(selection, unit.Context);
                    action.Precondition?.NotifyUsedCondition(selection, unit.Context);
                }

                unit.Context.Round++;
            }

            if (foundTactics == false)
                unit.ActionChargePercent -= 100f / unit.ActionChargeMax;
        }
        finally
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.UnlockReloadAssemblies();
#endif
            blocked &= ~BlockBattleFlags.UnitAct;
        }
    }

    public List<HeroExtension> PartyLineup => GameManager._instance._PartyLineup;

    public void SetOrderFor(CharacterTemplate character, Tactics specificTactic) => _orders[character] = specificTactic;

    void Update()
    {
        CleanUpdate();
    }

    // METHODS
    void SwitchCombatState(CombatState newCombatState)
    {
        switch(newCombatState)
        {
            case CombatState.START:
                _combatState = CombatState.START;
                OnNewStateSwitched?.Invoke(newCombatState);
                break;

            case CombatState.ACTIVE:
                _combatState = CombatState.ACTIVE;
                OnNewStateSwitched?.Invoke(newCombatState);
                break;

            case CombatState.WAIT:
                _combatState = CombatState.WAIT;
                OnNewStateSwitched?.Invoke(newCombatState);
                break;

            case CombatState.END:
                _combatState = CombatState.END;
                OnNewStateSwitched?.Invoke(newCombatState);
                break;
        }
    }

    #region CHANGE IN BATTLE STATE
    IEnumerator BattleIntermission(float x, CombatState combatState)
    {
        SwitchCombatState(combatState);
        yield return new WaitForSeconds(x);
        SwitchCombatState(CombatState.ACTIVE);
    }
    #endregion
    #region End of Battle
    IEnumerator VictoryTransition()
    {
        // Victory poses, exp gaining, items, transition back to overworld
        yield return new WaitForSeconds(1f);
        foreach (var yields in battleResolution.ResolveBattle(true, this))
            yield return yields;
    }
    IEnumerator DefeatTransition()
    {
        // Lost, Open up UI options to load saved game or return to title
        yield return new WaitForSeconds(1f);
        foreach (var yields in battleResolution.ResolveBattle(false, this))
            yield return yields;
    }
    internal static void ClearData()
    {
        #warning clean this stuff up
        foreach(EnemyExtension ext in GameManager._instance._EnemyLineup)
            Destroy(ext.gameObject);
        GameManager._instance._EnemyLineup.Clear();
    }
    #endregion
}


[Flags]
public enum BlockBattleFlags
{
    UnitAct = 0b0001,
    PreparingOrders = 0b0010,
}