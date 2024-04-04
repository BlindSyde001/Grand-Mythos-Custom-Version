using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Cinemachine;
using Conditions;
using Sirenix.OdinInspector;
using TMPro;
using Random = UnityEngine.Random;

public class BattleStateMachine : MonoBehaviour
{
    static BattleStateMachine _instance;

    public bool TurnBased;
    [NonSerialized]
    public BlockBattleFlags Blocked;

    public Team PlayerTeam;

    [Required]
    public BattleResolution BattleResolution;

    // VARIABLES
    public List<Transform> HeroSpawns;
    public List<Transform> EnemySpawns;

    public TMP_Text DebugNotificationText;

    [ReadOnly] public List<BattleCharacterController> PartyLineup = new();
    [ReadOnly] public List<BattleCharacterController> Units = new();
    [ReadOnly] public SerializableHashSet<BattleCharacterController> TacticsDisabled = new();

    /// <summary>
    /// The player-defined orders scheduled to run whenever the unit has the ability to do so
    /// </summary>
    public readonly Dictionary<BattleCharacterController, Tactics> Orders = new();

    public readonly Dictionary<BattleCharacterController, (Tactics chosenTactic, int actionI)> Processing = new();

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

            var controller = model.GetComponent<BattleCharacterController>();
            controller.Profile = hero;

            hero.ActionsCharged = Random.Range(0, hero.ActionChargeMax);
        }

        foreach (var target in FindObjectsOfType<BattleCharacterController>())
            if (Units.Contains(target) == false)
                Units.Add(target);

        yield return new WaitForSeconds(5);

        do
        {
            if (IsBattleFinished(out bool win))
            {
                _rotateCam.GetComponent<CinemachineFreeLook>().enabled = false;
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
                if (unit.Profile.CurrentHP == 0 || _busy.Contains(unit))
                    continue;

                bool processUnit = false;
                processUnit |= unit.Profile.ActionsCharged >= unit.Profile.ActionChargeMax && TacticsDisabled.Contains(unit) == false;
                processUnit |= Orders.TryGetValue(unit, out var chosenTactic) && chosenTactic.Condition.CanExecute(chosenTactic.Actions, new TargetCollection(Units), unit.Context, out _, accountForCost: true);

                // This unit has its ATB full or the manual order can be executed given the current amount of charge
                if (processUnit)
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

                if (unit.Profile.ActionsCharged > unit.Profile.ActionChargeMax)
                    unit.Profile.ActionsCharged = unit.Profile.ActionChargeMax;
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
            var allUnits = new TargetCollection(unitsCopy);
            if (Orders.TryGetValue(unit, out chosenTactic) && chosenTactic.Condition.CanExecute(chosenTactic.Actions, allUnits, unit.Context, out selection, true))
            {
                // We're taking care of this explicit order
            }
            else if (TacticsDisabled.Contains(unit))
            {
                yield break;
            }
            else
            {
                foreach (var tactic in unit.Profile.Tactics)
                {
                    if (tactic.IsOn && tactic.Condition.CanExecute(tactic.Actions, allUnits, unit.Context, out selection, true))
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

                    if (unit.Profile.ActionsCharged < action.ActionCost)
                        break;

                    if (i != 0)
                    {
                        // Check that our selection is still valid, may not be after running the previous action
                        if (false == chosenTactic.Condition.CanExecute(chosenTactic.Actions.AsSpan()[i..], allUnits, unit.Context, out selection, true))
                            break;
                        combinedSelection |= selection;
                    }

                    Processing[unit] = (chosenTactic, i);

                    if (unit.Profile.ActionAnimations.TryGet(action, out var animation) == false)
                    {
                        animation = unit.Profile.FallbackAnimation;
                        Debug.LogWarning($"No animations setup for action '{action}' on unit {unit}. Using fallback animation.", unit);
                    }

                    foreach (var yield in animation.Play(action, unit, selection.ToArray()))
                    {
                        yield return yield;
                        if (unit.Profile.EffectiveStats.HP == 0)
                            break;
                    }

                    // Check AGAIN that our selection is still valid, may not be after playing the animation
                    if (chosenTactic.Condition.CanExecute(chosenTactic.Actions.AsSpan()[i..], allUnits, unit.Context, out var newSelection, true))
                    {
                        selection = newSelection;
                        combinedSelection |= selection;
                    }
                    else
                    {
                        if (PlayerTeam != unit.Profile.Team)
                            break;

                        // Log to screen/user what went wrong here by re-evaluating with the tracker connected
                        try
                        {
                            var failureTracker = new FailureTracker();
                            unit.Context.Tracker = failureTracker;
                            chosenTactic.Condition.CanExecute(chosenTactic.Actions.AsSpan()[i..], allUnits, unit.Context, out _, true);
                            StartCoroutine(ShowFailureReason(failureTracker.FailureMessage));

                            IEnumerator ShowFailureReason(string text)
                            {
                                DebugNotificationText.gameObject.SetActive(true);
                                DebugNotificationText.text = text;
                                yield return new WaitForSeconds(10f);
                                DebugNotificationText.gameObject.SetActive(false);
                            }
                        }
                        finally
                        {
                            unit.Context.Tracker = null;
                        }

                        break;
                    }

                    action.Perform(selection.ToArray(), unit.Context);

                    unit.Profile.ActionsCharged -= action.ActionCost;

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

    class FailureTracker : IConditionEvalTracker
    {
        int _stackDepth;
        int _failureDepth = 0;
        public string FailureMessage;

        public void PostBeforeConditionEval(Condition condition, TargetCollection targetsBefore, EvaluationContext context)
        {
            _stackDepth++;
        }

        public void PostAfterConditionEval(Condition condition, TargetCollection targetsBefore, TargetCollection targetsAfter, EvaluationContext context)
        {
            // Only show the first failure that occured after a success
            if (targetsAfter.CountSlow() == 0 && targetsBefore.CountSlow() != 0 && _stackDepth > _failureDepth)
            {
                _failureDepth = _stackDepth;
                FailureMessage = $"Condition {condition.UIDisplayText} prevented {context.Profile.Name} to act";
            }

            _stackDepth--;
        }

        public void PostDead(CharacterTemplate source)
        {
            // Should be obvious enough not to mention it ?
        }

        public void PostTooCostly(CharacterTemplate source, ReadOnlySpan<IAction> actions)
        {
            string names = null;
            foreach (var action in actions)
                names = names is not null ? $", {action.Name}" : action.Name;

            FailureMessage = $"{source.Name} does not have enough charges to execute '{names}'";
        }

        public void PostActionPrecondition(CharacterTemplate source, IAction action, TargetCollection allTargets) { }

        public void PostActionTargetFilter(CharacterTemplate source, IAction action, TargetCollection previousTargets) { }

        public void PostTargetFilter(CharacterTemplate source, Condition targetFilter) { }

        public void PostAdditionalCondition(CharacterTemplate source, Condition condition, TargetCollection previousTargets) { }

        public void PostSuccess(CharacterTemplate source, TargetCollection previousTargets) { }
    }
}


[Flags]
public enum BlockBattleFlags
{
    PreparingOrders = 0b0001,
}