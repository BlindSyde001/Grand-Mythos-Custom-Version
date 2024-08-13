using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using Conditions;
using Sirenix.OdinInspector;
using TMPro;
using Random = UnityEngine.Random;

public class BattleStateMachine : MonoBehaviour
{
    static BattleStateMachine _instance;

    public bool TurnBased;
    public bool AllowHostileActionWhilePlayerIdles = true;
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

    /// <summary>
    /// The player-defined orders scheduled to run whenever the unit has the ability to do so
    /// </summary>
    public readonly Dictionary<BattleCharacterController, Tactics> Orders = new();

    public readonly HashSet<BattleCharacterController> Processing = new();

    public readonly List<BattleCharacterController> Queue = new();

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
        InputManager.PushGameState(GameState.Battle, this);
    }

    void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
        InputManager.PopGameState(this);
    }

    IEnumerator Start()
    {
        foreach (var target in FindObjectsOfType<BattleCharacterController>())
        {
            if (target.Profile is HeroExtension hero && GameManager.Instance.PartyLineup.Contains(hero))
                PartyLineup.Add(target);

            if (Units.Contains(target) == false)
            {
                target.Profile.Pause = Random.Range(0, 1);
                Units.Add(target);
            }
        }

        // Sort them in the order they are setup in the party
        PartyLineup = GameManager.Instance.PartyLineup.Select(x => PartyLineup.FirstOrDefault(y => y.Profile == x)).Where(x => x != null).ToList();

        yield return new WaitForSeconds(5);

        do
        {
            if (IsBattleFinished(out bool win))
            {
                enabled = false;
                yield return new WaitForSeconds(1f);
                foreach (var yields in BattleResolution.ResolveBattle(win, this))
                    yield return yields;
                yield break;
            }
            
            if (Blocked != 0)
            {
                if (Blocked == BlockBattleFlags.PreparingOrders && Settings.Current.BattleCommandSpeed != Settings.BattleCommandSpeedType.Pause)
                {
                    
                }
                else
                {
                    yield return null; // Wait for next frame
                    continue;
                }
            }

            while (Queue.Count > 0)
            {
                var unit = Queue[0];
                if (unit.Profile.CurrentHP == 0 || unit.Profile.Pause > 0) // Remove any invalid units
                {
                    Queue.RemoveAt(0);
                    continue;
                }

                if (unit.Profile.Team == PlayerTeam && Orders.TryGetValue(unit, out _) == false)
                {
                    if (AllowHostileActionWhilePlayerIdles && Queue.FirstOrDefault(x => x.Profile.Team != PlayerTeam) is { } hostile)
                    {
                        Queue.Remove(hostile);
                        Queue.Insert(0, hostile);
                        continue;
                    }
                    else
                    {
                        break; // just wait while the player idles
                    }
                }

                Queue.RemoveAt(0);

                Processing.Add(unit);
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

            var battleDeltaTime = Blocked == BlockBattleFlags.PreparingOrders && Settings.Current.BattleCommandSpeed == Settings.BattleCommandSpeedType.Slow ? 0.5f : 1f;
            battleDeltaTime = Time.deltaTime * battleDeltaTime * Settings.Current.BattleSpeed / 100f;
            foreach (var unit in Units)
            {
                if (Processing.Contains(unit) == false && unit.Profile.CurrentHP != 0)
                {
                    unit.Profile.Pause = MathF.Max(unit.Profile.Pause - unit.Profile.ActionRechargeSpeed * battleDeltaTime, 0f);
                    if (unit.Profile.Pause == 0f && Queue.Contains(unit) == false)
                        Queue.Add(unit);
                }

                using (unit.Context.EnmityTowards.TemporaryCopy(out var copy))
                {
                    foreach (var character in copy)
                    {
                        var baseValue = unit.Context.EnmityTowards[character];
                        baseValue -= battleDeltaTime * SingletonManager.Instance.Formulas.EnmityDecay;
                        baseValue = baseValue < 0 ? 0 : baseValue;
                        unit.Context.EnmityTowards[character] = baseValue;
                    }
                }
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

    class SortBasedOnEnmity : IComparer<BattleCharacterController>
    {
        private Dictionary<CharacterTemplate, float> _enmity;
        public SortBasedOnEnmity(Dictionary<CharacterTemplate, float> enmity)
        {
            _enmity = enmity;
        }

        public int Compare(BattleCharacterController x, BattleCharacterController y)
        {
            _enmity.TryGetValue(x.Profile, out float enmityX);
            _enmity.TryGetValue(y.Profile, out float enmityY);
            return -enmityX.CompareTo(enmityY);
        }
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
            using var __ = Units.TemporaryCopy(out var unitsCopy);
            
            if (unit.Profile is HeroExtension == false)
                unitsCopy.Sort(new SortBasedOnEnmity(unit.Context.EnmityTowards));

            Tactics chosenTactic;
            TargetCollection selection = default;
            var allUnits = new TargetCollection(unitsCopy);
            if (Orders.TryGetValue(unit, out chosenTactic) && chosenTactic.Condition.CanExecute(chosenTactic.Action, allUnits, unit.Context, out selection))
            {
                // We're taking care of this explicit order
            }
            else
            {
                foreach (var tactic in unit.Profile.Tactics)
                {
                    if (tactic != null && tactic.IsOn && tactic.Condition.CanExecute(tactic.Action, allUnits, unit.Context, out selection))
                    {
                        chosenTactic = tactic;
                        break;
                    }
                }
            }
            Orders.Remove(unit);
            Processing.Add(unit);

            if (chosenTactic == null) 
                yield break;

            var combinedSelection = selection;
            {
                var action = chosenTactic.Action;

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
                if (chosenTactic.Condition.CanExecute(chosenTactic.Action, allUnits, unit.Context, out var newSelection))
                {
                    selection = newSelection;
                    combinedSelection |= selection;
                }
                else
                {
                    if (PlayerTeam != unit.Profile.Team)
                        yield break;

                    // Log to screen/user what went wrong here by re-evaluating with the tracker connected
                    try
                    {
                        var failureTracker = new FailureTracker(chosenTactic.Action);
                        unit.Context.Tracker = failureTracker;
                        chosenTactic.Condition.CanExecute(chosenTactic.Action, allUnits, unit.Context, out _);
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

                    yield break;
                }
                action.Perform(selection.ToArray(), unit.Context);

                float enmityGain = Mathf.Pow(action.EnmityGenerationTarget, SingletonManager.Instance.Formulas.EnmityGainScaling);
                float enmityGainNonTargets = Mathf.Pow(action.EnmityGenerationNonTarget, SingletonManager.Instance.Formulas.EnmityGainScaling);
                foreach (var controller in Units)
                {
                    if (controller.IsHostileTo(unit) == false)
                        continue;

                    controller.Context.EnmityTowards.TryGetValue(unit.Profile, out var val);
                    controller.Context.EnmityTowards[unit.Profile] = val + (selection.Contains(controller) ? enmityGain : enmityGainNonTargets);
                }

                unit.Profile.Pause += 1f;
            }

            chosenTactic.Condition.TargetFilter?.NotifyUsedCondition(combinedSelection, unit.Context);
            chosenTactic.Condition.AdditionalCondition?.NotifyUsedCondition(combinedSelection, unit.Context);
            chosenTactic.Action.TargetFilter?.NotifyUsedCondition(combinedSelection, unit.Context);
            chosenTactic.Action.Precondition?.NotifyUsedCondition(combinedSelection, unit.Context);
        }
        finally
        {
            unit.Context.Round++;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.UnlockReloadAssemblies();
#endif
            Processing.Remove(unit);
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
        PartyLineup.Remove(unit);
    }

    class FailureTracker : IConditionEvalTracker
    {
        int _stackDepth;
        int _failureDepth = 0;
        public string FailureMessage;
        IAction _associatedAction;

        public FailureTracker(IAction associatedAction)
        {
            _associatedAction = associatedAction;
        }

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
                FailureMessage = $"Condition {condition.UIDisplayText} prevented {context.Profile.Name} to use {string.Join(',', _associatedAction.Name)}";
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

public partial class Settings
{
    public float BattleSpeed = 1f;
    public BattleCommandSpeedType BattleCommandSpeed = BattleCommandSpeedType.Pause;

    public enum BattleCommandSpeedType
    {
        Pause,
        Slow,
        Active,
    }
}

[Flags]
public enum BlockBattleFlags
{
    PreparingOrders = 0b0001,
}