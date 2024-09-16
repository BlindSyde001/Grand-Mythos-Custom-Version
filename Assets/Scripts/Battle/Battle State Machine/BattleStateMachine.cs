using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Characters.StatusHandler;
using UnityEngine;
using Conditions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.Serialization;
using Random = Unity.Mathematics.Random;

public class BattleStateMachine : MonoBehaviour
{
    static BattleStateMachine _instance;

    public bool AllowHostileActionWhilePlayerIdles = true;
    [NonSerialized]
    public BlockBattleFlags Blocked;

    public Team PlayerTeam;

    public AnimationClip Intro, Outro;
    
    [Required]
    public BattleResolution BattleResolution;

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

    private Random _random = new Random(10);
    private double _timestamp = 0;

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
        const float SlowdownFactor = 0.25f;

        foreach (var target in FindObjectsOfType<BattleCharacterController>())
            Include(target);

        if (Intro)
        {
            BattleCamera.Instance.PlayUninterruptible(Intro);
            for (float f = 0; f < Intro.length; f += Time.deltaTime)
                yield return null;
        }

        // Sort them in the order they are setup in the party
        PartyLineup = GameManager.Instance.PartyLineup.Select(x => PartyLineup.FirstOrDefault(y => y.Profile == x)).Where(x => x != null).ToList();

        var chargingUnits = new List<(BattleCharacterController unit, Tactics tactic, List<BattleCharacterController> targets)>();
        bool win;
        IEnumerator busy = null;
        while(IsBattleFinished(out win) == false)
        {
            Time.timeScale = 1f;

            if (Blocked != 0)
            {
                if (Blocked == BlockBattleFlags.PreparingOrders && Settings.Current.BattleSelectionType != BattleSelectionType.Pause)
                {
                    
                }
                else
                {
                    Time.timeScale = SlowdownFactor;
                    yield return null; // Wait for next frame
                    continue;
                }
            }

            if (busy is not null)
            {
                if (busy.MoveNext() == false)
                    busy = null;
            }

            for (int i = 0; i < chargingUnits.Count && busy is null; i++)
            {
                var (unit, tactic, targets) = chargingUnits[i];
                if (unit.Profile.CurrentHP == 0)
                {
                    unit.Profile.ChargeLeft = 0f;
                    chargingUnits.RemoveAt(i--);
                    continue;
                }
                if (unit.Profile.ChargeLeft != 0)
                    continue;

                unit.Profile.ChargeTotal = 0f;
                Processing.Add(unit);
                var enumerator = CatchException(ProcessUnit(unit, tactic, targets));
                
                if (Settings.Current.BattleTurnType == BattleTurnType.Sequential && tactic.Action.Channeling is null)
                {
                    busy = enumerator;
                }
                else
                {
                    StartCoroutine(enumerator);
                }

                chargingUnits.RemoveAt(i--);
            }

            while (busy is null && Queue.Count > 0)
            {
                var unit = Queue[0];
                if (unit.Profile.CurrentHP == 0 || unit.Profile.PauseLeft > 0) // Remove any invalid units
                {
                    Queue.RemoveAt(0);
                    continue;
                }

                Tactics chosenTactic;
                List<BattleCharacterController> selection;
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
                    
                TargetCollection selectionAsTargetCollection = default;
                // Find the right tactic and targets to use for this unit
                using (Units.TemporaryCopy(out var unitsCopy))
                {
                    if (unit.Profile.Modifiers.FirstOrDefault(x => x.Modifier is TauntModifier) is { Modifier: not null } taunt)
                        unitsCopy.RemoveAll(x => x.Profile != taunt.Source);

                    var allUnits = new TargetCollection(unitsCopy);
                    if (Orders.Remove(unit, out chosenTactic))
                    {
                        if (chosenTactic.Condition.CanExecute(chosenTactic.Action, allUnits, unit.Context, out selectionAsTargetCollection) == false)
                            continue; // This order is no longer actionable, skip this unit
                    }
                    else
                    {
                        foreach (var tactic in unit.Profile.Tactics)
                        {
                            if (tactic != null && tactic.IsOn && tactic.Condition.CanExecute(tactic.Action, allUnits, unit.Context, out selectionAsTargetCollection))
                            {
                                chosenTactic = tactic;
                                break;
                            }
                        }
                    }

                    selection = selectionAsTargetCollection.ToList();
                }

                if (chosenTactic.Action.ChargeDuration > 0)
                {
                    unit.Profile.ChargeLeft = unit.Profile.ChargeTotal = chosenTactic.Action.ChargeDuration;
                    chargingUnits.Add((unit, chosenTactic, selection));
                    continue;
                }

                if (chosenTactic == null) 
                    continue;

                Processing.Add(unit);
                var enumerator = CatchException(ProcessUnit(unit, chosenTactic, selection));
                if (Settings.Current.BattleTurnType == BattleTurnType.Sequential && chosenTactic.Action.Channeling is null)
                {
                    busy = enumerator;
                }
                else
                {
                    StartCoroutine(enumerator);
                }
            }

            if (busy is null
                && Blocked == BlockBattleFlags.PreparingOrders
                && Settings.Current.BattleSelectionType == BattleSelectionType.Slow)
            {
                Time.timeScale = SlowdownFactor;
            }
            else
            {
                Time.timeScale = 1f;
            }
            float battleDeltaTime = Time.deltaTime * Settings.Current.BattleSpeed;
            _timestamp += battleDeltaTime;

            foreach (var (unit, _, _) in chargingUnits)
            {
                unit.Profile.ChargeLeft = MathF.Max(unit.Profile.ChargeLeft - battleDeltaTime, 0f);
            }

            foreach (var unit in Units)
            {
                if (Processing.Contains(unit) == false && unit.Profile.CurrentHP != 0)
                {
                    unit.Profile.PauseLeft = MathF.Max(unit.Profile.PauseLeft - unit.Profile.ActionRechargeSpeed * battleDeltaTime / 100f, 0f);
                    if (unit.Profile.PauseLeft == 0f && Queue.Contains(unit) == false && chargingUnits.Exists( x => x.unit == unit) == false)
                        Queue.Add(unit);
                }

                if (unit.Profile.InFlowState)
                {
                    unit.Profile.CurrentFlow -= battleDeltaTime * SingletonManager.Instance.Formulas.FlowDepletionRate;
                    if (unit.Profile.CurrentFlow <= 0f)
                    {
                        unit.Profile.InFlowState = false;
                        unit.Profile.CurrentFlow = 0f;
                    }
                }

                unit.Context.CombatTimestamp = _timestamp;

                for (int i = unit.Profile.Modifiers.Count - 1; i >= 0; i--)
                {
                    var m = unit.Profile.Modifiers[i];
                    if (m.Modifier.IsStillValid(m, unit.Context) == false)
                        unit.Profile.Modifiers.RemoveAt(i);
                }
            }

            yield return null; // Wait for next frame
        }
        
        foreach (var unit in PartyLineup)
        {
            for (int i = unit.Profile.Modifiers.Count - 1; i >= 0; i--)
            {
                var m = unit.Profile.Modifiers[i];
                if (m.Modifier.Temporary && Settings.Current.ModifiersBehavior == ModifiersBehavior.RemovedAfterBattle)
                    unit.Profile.Modifiers.RemoveAt(i);
            }
        }
                
        enabled = false;

        if (Outro)
        {
            BattleCamera.Instance.PlayUninterruptible(Outro, false);
            for (float f = 0; f < Outro.length; f += Time.deltaTime)
                yield return null;
            BattleCamera.Instance.enabled = false;
        }
        
        foreach (var yields in BattleResolution.ResolveBattle(win, this))
            yield return yields;
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

    IEnumerable ProcessUnit(BattleCharacterController unit, Tactics chosenTactic, List<BattleCharacterController> preselection)
    {
        #if UNITY_EDITOR
        // Halting execution to reload assemblies while this enumerator is running
        // may put the state of the combat into an unrecoverable state, make sure that doesn't happen
        UnityEditor.EditorApplication.LockReloadAssemblies();
        #endif
        try
        {
            if (unit.Profile.ActionAnimations.TryGet(chosenTactic.Action, out var animation) == false)
            {
                animation = unit.Profile.FallbackAnimation;
                Debug.LogWarning($"No animations setup for action '{chosenTactic.Action}' on unit {unit}. Using fallback animation.", unit);
            }

            double startingTimestamp = _timestamp;
            int execCount = 0;

            {
                if (chosenTactic.Action.Channeling is { } channeling2)
                    unit.Profile.ChargeLeft = unit.Profile.ChargeTotal = channeling2.Duration;
            }
            AGAIN:

            if (chosenTactic.Action.CameraAnimation)
            {
                BattleCamera.Instance.TryPlayAnimation(unit, chosenTactic.Action.CameraAnimation);
            }

            foreach (var yield in animation.Play(chosenTactic.Action, unit, preselection.ToArray()))
            {
                yield return yield;
                unit.Profile.ChargeLeft = (float)(_timestamp - startingTimestamp);
                if (unit.Profile.EffectiveStats.HP == 0)
                    break;
            }
            
            using (Units.TemporaryCopy(out var unitsCopy))
            {
                var allUnits = new TargetCollection(unitsCopy);
                // Check AGAIN that our selection is still valid, may not be after playing the animation
                if (chosenTactic.Condition.CanExecute(chosenTactic.Action, allUnits, unit.Context, out var selection) == false)
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
                chosenTactic.Action.Perform(selection.ToArray(), unit.Context);

                chosenTactic.Condition.TargetFilter?.NotifyUsedCondition(selection, unit.Context);
                chosenTactic.Condition.AdditionalCondition?.NotifyUsedCondition(selection, unit.Context);
                chosenTactic.Action.TargetFilter?.NotifyUsedCondition(selection, unit.Context);
                chosenTactic.Action.Precondition?.NotifyUsedCondition(selection, unit.Context);
            }

            if (chosenTactic.Action.Channeling is {} channeling && ++execCount < channeling.Ticks)
            {
                double deltaBetweenTicks = channeling.Duration / channeling.Ticks;
                double nextTick = startingTimestamp + execCount * deltaBetweenTicks;
                unit.Profile.ChargeLeft = (float)(_timestamp - startingTimestamp);
                while (_timestamp < nextTick)
                {
                    yield return null;
                    unit.Profile.ChargeLeft = (float)(_timestamp - startingTimestamp);
                }

                goto AGAIN;
            }

            unit.Profile.PauseLeft += 1f;
        }
        finally
        {
            unit.Context.Round++;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.UnlockReloadAssemblies();
#endif
            Processing.Remove(unit);
            unit.Profile.ChargeTotal = unit.Profile.ChargeLeft = 0f;
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
        {
            Units.Add(unit);
            unit.Profile.PauseLeft = _random.NextFloat(0, 1);
        }

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

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1, 0.25f);
        foreach (var spawn in HeroSpawns)
            Gizmos.DrawCube(spawn.position + Vector3.up, new Vector3(1, 2, 1));
        foreach (var spawn in EnemySpawns)
            Gizmos.DrawCube(spawn.position + Vector3.up, new Vector3(1, 2, 1));
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

        public void PostNotEnoughMana(CharacterTemplate source)
        {
            FailureMessage = $"{source.Name} does not have enough mana to perform this action";
        }

        public void PostNotEnoughFlow(CharacterTemplate source)
        {
            FailureMessage = $"{source.Name} does not have enough flow to perform this action";
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
    [FormerlySerializedAs("BattleMenuMode")] public BattleSelectionType BattleSelectionType = BattleSelectionType.Pause;
    public BattleTurnType BattleTurnType = BattleTurnType.Sequential;
    public ModifiersBehavior ModifiersBehavior = ModifiersBehavior.RemovedAfterBattle;
}

[Flags]
public enum BlockBattleFlags
{
    PreparingOrders = 0b0001,
    DetailedInfoOpen = 0b0010,
}

public enum BattleSelectionType
{
    Pause,
    Slow,
    FullSpeed,
}

public enum BattleTurnType
{
    Sequential,
    Concurrent,
}

public enum ModifiersBehavior
{
    RemovedAfterBattle,
    CarriedBetweenBattles
}