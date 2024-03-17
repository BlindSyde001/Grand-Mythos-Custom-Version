using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Cinemachine;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

public class BattleStateMachine : MonoBehaviour
{
    public delegate void SwitchToNewState(CombatState state);
    public static event SwitchToNewState OnNewStateSwitched;
    static BattleStateMachine _instance;

    public bool TurnBased;
    [NonSerialized]
    public BlockBattleFlags Blocked;

    public Team PlayerTeam;

    [Required]
    public BattleResolution BattleResolution;

    // VARIABLES
    public List<Transform> HeroSpawns;    // Where do they initially spawn?
    public List<Transform> EnemySpawns;

    [ReadOnly] public List<BattleCharacterController> PartyLineup = new();
    [ReadOnly] public List<BattleCharacterController> Units = new();
    [ReadOnly] public SerializableHashSet<BattleCharacterController> TacticsDisabled = new();

    /// <summary>
    /// The player-defined orders scheduled to run whenever the unit has the ability to do so
    /// </summary>
    public readonly Dictionary<BattleCharacterController, Tactics> Orders = new();

    public readonly Dictionary<BattleCharacterController, (Tactics chosenTactic, int actionI)> Processing = new();

    CombatState _combatState;
    CinemachineFreeLook _rotateCam;
    HashSet<BattleCharacterController> _busy = new();

    // UPDATES
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this);
            return;
        }
        _rotateCam = FindObjectOfType<CinemachineFreeLook>();
        InputManager.Instance.PushGameState(GameState.Battle, this);
    }

    void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
        InputManager.Instance.PopGameState(this);
    }

    IEnumerator Start()
    {
        var gameManager = GameManager.Instance;
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
            controller.Profile = hero;

            hero.ActionsCharged = Random.Range(0, hero.ActionChargeMax);
        }

        foreach (var target in FindObjectsOfType<BattleCharacterController>())
            if (Units.Contains(target) == false)
                Units.Add(target);

        SendStateChangeNotification(CombatState.Start);
        yield return new WaitForSeconds(5);
        SendStateChangeNotification(CombatState.Active);

        do
        {
            if (IsBattleFinished(out bool win))
            {
                _rotateCam.GetComponent<CinemachineFreeLook>().enabled = false;
                SendStateChangeNotification(CombatState.End);
                enabled = false;
                yield return new WaitForSeconds(1f);
                foreach (var yields in BattleResolution.ResolveBattle(win, this))
                    yield return yields;
                yield break;
            }

            if (Blocked != 0)
            {
                yield return null; // Wait for next frame
                continue;
            }

            foreach (var unit in Units)
            {
                if (unit.Profile.CurrentHP == 0 || TacticsDisabled.Contains(unit) || _busy.Contains(unit))
                    continue;

                // This unit has its ATB full or the manual order can be executed given the current amount of charge
                if (unit.Profile.ActionsCharged >= unit.Profile.ActionChargeMax || Orders.TryGetValue(unit, out var chosenTactic) && chosenTactic.Condition.CanExecute(chosenTactic.Actions, new TargetCollection(Units), unit.Context, out _, accountForCost: true))
                {
                    _busy.Add(unit);
                    if (TurnBased)
                    {
                        for (var enumerator = CatchException(ProcessUnit(unit)); enumerator.MoveNext(); )
                            yield return enumerator.Current;
                    }
                    else
                    {
                        StartCoroutine(CatchException(ProcessUnit(unit)));
                    }
                }
            }

            foreach (var unit in Units)
            {
                if (unit.Profile.CurrentHP != 0 && _busy.Contains(unit) == false)
                    unit.Profile.ActionsCharged += unit.Profile.ActionRechargeSpeed * Time.deltaTime / 10f;
                // DO NOT CLAMP HERE, DO IT AFTER ORDERS HAVE BEEN SCHEDULED
            }

            yield return null; // Wait for next frame
        } while (true);
    }

    IEnumerator CatchException(IEnumerable enumerable)
    {
        IEnumerator enumerator = null;
        try
        {
            for (enumerator = enumerable.GetEnumerator(); ; )
            {
                object yield;
                try
                {
                    if (enumerator.MoveNext() == false)
                        break;
                    yield = enumerator.Current;
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                    break;
                }

                yield return yield;
            }
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }


    bool IsBattleFinished(out bool win)
    {
        int alliesLeft = 0;
        int hostilesLeft = 0;
        foreach (var target in Units)
        {
            if (target.Profile.CurrentHP == 0)
                continue;
            if (PlayerTeam.Allies.Contains(target.Profile.Team))
                alliesLeft++;
            else
                hostilesLeft++;
        }

        win = hostilesLeft == 0 && alliesLeft > 0;
        return hostilesLeft == 0 || alliesLeft == 0;
    }

    IEnumerable ProcessUnit(BattleCharacterController unit)
    {
        #if UNITY_EDITOR
        // Halting execution to reload assemblies while this enumerator is running
        // may put the state of the combat into an unrecoverable state, make sure that doesn't happen
        UnityEditor.EditorApplication.LockReloadAssemblies();
        #endif
        try
        {
            Tactics chosenTactic;
            TargetCollection selection = default;
            using var __ = Units.TemporaryCopy(out var unitsCopy);
            if (Orders.TryGetValue(unit, out chosenTactic) && chosenTactic.Condition.CanExecute(chosenTactic.Actions, new TargetCollection(unitsCopy), unit.Context, out selection, true))
            {
                // We're taking care of this explicit order
            }
            else
            {
                foreach (var tactic in unit.Profile.Tactics)
                {
                    if (tactic.IsOn && tactic.Condition.CanExecute(tactic.Actions, new TargetCollection(unitsCopy), unit.Context, out selection, true))
                    {
                        chosenTactic = tactic;
                        break;
                    }
                }
            }
            Orders.Remove(unit);

            if (chosenTactic != null)
            {
                var combinedSelection = selection;
                for (int i = 0; i < chosenTactic.Actions.Length; i++)
                {
                    var action = chosenTactic.Actions[i];

                    if (unit.Profile.ActionsCharged < action.ATBCost)
                        break;

                    if (i != 0)
                    {
                        // Check that our selection is still valid, may not be after running the previous action
                        var actionLeft = chosenTactic.Actions.AsSpan()[i..];
                        if (chosenTactic.Condition.CanExecute(actionLeft, selection, unit.Context, out _, true) == false)
                        {
                            // If not, check if we have something else to target
                            if (chosenTactic.Condition.CanExecute(actionLeft, new TargetCollection(unitsCopy), unit.Context, out var newSelection, true))
                            {
                                // Continue with that selection instead
                                selection = newSelection;
                                combinedSelection |= newSelection;
                            }
                            else
                                break;
                        }
                    }

                    Processing[unit] = (chosenTactic, i);

                    if (unit.Profile.ActionAnimations.TryGet(action, out var animation) == false)
                    {
                        animation = unit.Profile.FallbackAnimation;
                        Debug.LogWarning($"No animations setup for action '{action}' on unit {unit}. Using fallback animation.", unit);
                    }

                    foreach (var yield in animation.Play(action, unit, selection.ToArray()))
                        yield return yield;

                    foreach (var yield in action.Perform(selection.ToArray(), unit.Context))
                        yield return yield;

                    unit.Profile.ActionsCharged -= action.ATBCost;

                    if (Units.Contains(unit) == false)
                        break;
                }

                chosenTactic.Condition.TargetFilter?.NotifyUsedCondition(combinedSelection, unit.Context);
                chosenTactic.Condition.AdditionalCondition?.NotifyUsedCondition(combinedSelection, unit.Context);
                foreach (var action in chosenTactic.Actions)
                {
                    action.TargetFilter?.NotifyUsedCondition(combinedSelection, unit.Context);
                    action.Precondition?.NotifyUsedCondition(combinedSelection, unit.Context);
                }

                unit.Context.Round++;
            }

            if (unit.Profile.ActionsCharged > unit.Profile.ActionChargeMax)
                unit.Profile.ActionsCharged = unit.Profile.ActionChargeMax;
        }
        finally
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.UnlockReloadAssemblies();
#endif
            Processing.Remove(unit);
            _busy.Remove(unit);
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

    public void Include(BattleCharacterController unit)
    {
        if (unit == null)
            throw new NullReferenceException(nameof(unit));

        if (Units.Contains(unit) == false)
            Units.Add(unit);

#warning clean this up
        PartyLineup.Clear();
        foreach (var heroExtension in GameManager.Instance.PartyLineup)
        {
            foreach (var controller in Units)
            {
                if (controller.Profile == heroExtension)
                {
                    PartyLineup.Add(controller);
                    break;
                }
            }
        }
    }

    public void Exclude(BattleCharacterController unit)
    {
        if (unit == null)
            return;
        Units.Remove(unit);

#warning clean this up
        PartyLineup.Clear();
        foreach (var heroExtension in GameManager.Instance.PartyLineup)
        {
            foreach (var controller in Units)
            {
                if (controller.Profile == heroExtension)
                {
                    PartyLineup.Add(controller);
                    break;
                }
            }
        }
    }

    // METHODS
    void SendStateChangeNotification(CombatState newCombatState)
    {
        try
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(newCombatState), newCombatState, null);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    #region End of Battle
    internal static void ClearData()
    {
        #warning clean this stuff up
        foreach (var hostile in FindObjectsOfType<BattleCharacterController>())
        {
            if (hostile.Profile != null && hostile.Profile is not HeroExtension)
                Destroy(hostile.Profile);
            Destroy(hostile.gameObject);
        }
    }
    #endregion
}


[Flags]
public enum BlockBattleFlags
{
    PreparingOrders = 0b0001,
}