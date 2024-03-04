using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Cinemachine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class BattleStateMachine : MonoBehaviour
{
    static BattleStateMachine _instance;

    public Team PlayerTeam;
    public SerializableHashSet<CharacterTemplate> Units = new();
    public SerializableHashSet<CharacterTemplate> TacticsDisabled = new();

    [NonSerialized]
    public BlockBattleFlags Blocked;

    List<CharacterTemplate> _unitsCopy = new();
    Dictionary<CharacterTemplate, Tactics> _orders = new();
    [SerializeField] BattleResolution _battleResolution;

    // VARIABLES
    public List<Transform> HeroSpawns;    // Where do they initially spawn?
    public List<Transform> EnemySpawns;

    public delegate void SwitchToNewState(CombatState state);
    public static event SwitchToNewState OnNewStateSwitched;

    CombatState _combatState;
    CinemachineFreeLook _rotateCam;

    public List<HeroExtension> PartyLineup => GameManager._instance.PartyLineup;

    // UPDATES
    void Awake()
    {
        _rotateCam = FindObjectOfType<CinemachineFreeLook>();
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this);
            return;
        }
    }

    void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    void Start()
    {
        var gameManager = GameManager._instance;
        foreach (var reserve in gameManager.ReservesLineup)
            reserve.gameObject.SetActive(false);

        for (int i = 0; i < gameManager.PartyLineup.Count; i++)
        {
            var hero = gameManager.PartyLineup[i];

            // Add Model into Battle
            var model = Instantiate(hero.BattlePrefab,
                HeroSpawns[i].position,
                HeroSpawns[i].rotation,
                transform.parent);

            model.name = $"{hero.gameObject.name} Model";

            var controller = model.GetComponent<BattleHeroModelController>();
            controller.Template = hero;

            hero.ActionsCharged = Random.Range(0, hero.ActionChargeMax);
        }

        SwitchCombatState(CombatState.Start);
        StartCoroutine(BattleIntermission(5, CombatState.Start));
    }

    void OnEnable()
    {
        foreach (var target in FindObjectsOfType<CharacterTemplate>())
            Units.Add(target);
    }

    void Update()
    {
        if (Blocked != 0)
            return;

        int alliesLeft = 0;
        int hostilesLeft = 0;
        foreach (var target in Units)
        {
            if (target.CurrentHP == 0)
                continue;
            if (PlayerTeam.Allies.Contains(target.Team))
                alliesLeft++;
            else
                hostilesLeft++;
        }

        if (hostilesLeft == 0)
        {
            _rotateCam.GetComponent<CinemachineFreeLook>().enabled = false;
            SwitchCombatState(CombatState.End);
            enabled = false;
            StartCoroutine(VictoryTransition());
        }
        else if (alliesLeft == 0)
        {
            _rotateCam.GetComponent<CinemachineFreeLook>().enabled = false;
            SwitchCombatState(CombatState.End);
            enabled = false;
            StartCoroutine(DefeatTransition());
        }

        _unitsCopy.Clear();
        _unitsCopy.AddRange(Units);
        foreach (var unit in Units)
        {
            if (unit.CurrentHP == 0 || TacticsDisabled.Contains(unit))
                continue;

            // This unit has its ATB full or the manual order can be executed given the current amount of charge
            if (unit.ActionsCharged >= unit.ActionChargeMax || _orders.TryGetValue(unit, out var chosenTactic) && chosenTactic.Condition.CanExecuteWithAction(chosenTactic.Actions, new TargetCollection(_unitsCopy), unit.Context, out _, accountForCost:true))
            {
                Blocked |= BlockBattleFlags.UnitAct;
                StartCoroutine(ProcessUnit(unit));
                return; // Explicitly return to prevent others from acting at the same time
            }
        }

        foreach (var unit in Units)
        {
            if (unit.CurrentHP != 0)
                unit.ActionsCharged += unit.ActionRechargeSpeed * Time.deltaTime / 10f;
            // DO NOT CLAMP HERE, DO IT AFTER ORDERS HAVE BEEN SCHEDULED
        }
    }

    IEnumerator ProcessUnit(CharacterTemplate unit)
    {
        Blocked |= BlockBattleFlags.UnitAct;
        #if UNITY_EDITOR
        // Halting execution to reload assemblies while this enumerator is running
        // may put the state of the combat into an unrecoverable state, make sure that doesn't happen
        UnityEditor.EditorApplication.LockReloadAssemblies();
        #endif
        try
        {
            bool foundTactics = false;
            Tactics chosenTactic;
            TargetCollection selection = default;
            if (_orders.TryGetValue(unit, out chosenTactic) && chosenTactic.Condition.CanExecuteWithAction(chosenTactic.Actions, new TargetCollection(_unitsCopy), unit.Context, out selection, true))
            {
                // We're taking care of this explicit order
            }
            else
            {
                foreach (var tactic in unit.Tactics)
                {
                    if (tactic.IsOn && tactic.Condition.CanExecuteWithAction(tactic.Actions, new TargetCollection(_unitsCopy), unit.Context, out selection, true))
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
                    if (unit.ActionsCharged < action.ATBCost)
                        break;

                    foundTactics = true;
                    unit.ActionsCharged -= action.ATBCost;

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
                unit.ActionsCharged -= 1f;
        }
        finally
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.UnlockReloadAssemblies();
#endif
            Blocked &= ~BlockBattleFlags.UnitAct;
        }
    }

    public static bool TryGetInstance([MaybeNullWhen(false)] out BattleStateMachine bts)
    {
        if (_instance != null)
        {
            bts = _instance;
            return true;
        }

        bts = null;
        return false;
    }

    public void Include(CharacterTemplate unit)
    {
        if (unit == null)
            throw new NullReferenceException(nameof(unit));
        Units.Add(unit);
    }

    public void Exclude(CharacterTemplate unit)
    {
        if (unit == null)
            return;
        Units.Remove(unit);
    }

    public void SetOrderFor(CharacterTemplate character, Tactics specificTactic) => _orders[character] = specificTactic;

    // METHODS
    void SwitchCombatState(CombatState newCombatState)
    {
        switch(newCombatState)
        {
            case CombatState.Start:
                _combatState = CombatState.Start;
                OnNewStateSwitched?.Invoke(newCombatState);
                break;

            case CombatState.Active:
                _combatState = CombatState.Active;
                OnNewStateSwitched?.Invoke(newCombatState);
                break;

            case CombatState.Wait:
                _combatState = CombatState.Wait;
                OnNewStateSwitched?.Invoke(newCombatState);
                break;

            case CombatState.End:
                _combatState = CombatState.End;
                OnNewStateSwitched?.Invoke(newCombatState);
                break;
        }
    }

    IEnumerator BattleIntermission(float x, CombatState combatState)
    {
        SwitchCombatState(combatState);
        yield return new WaitForSeconds(x);
        SwitchCombatState(CombatState.Active);
    }

    #region End of Battle
    IEnumerator VictoryTransition()
    {
        // Victory poses, exp gaining, items, transition back to overworld
        yield return new WaitForSeconds(1f);
        foreach (var yields in _battleResolution.ResolveBattle(true, this))
            yield return yields;
    }
    IEnumerator DefeatTransition()
    {
        // Lost, Open up UI options to load saved game or return to title
        yield return new WaitForSeconds(1f);
        foreach (var yields in _battleResolution.ResolveBattle(false, this))
            yield return yields;
    }
    internal static void ClearData()
    {
        #warning clean this stuff up
        foreach (var hostile in FindObjectsOfType<BattleCharacterController>())
        {
            if (hostile.Template != null && hostile.Template is not HeroExtension)
                Destroy(hostile.Template);
            Destroy(hostile.gameObject);
        }
    }
    #endregion
}


[Flags]
public enum BlockBattleFlags
{
    UnitAct = 0b0001,
    PreparingOrders = 0b0010,
}