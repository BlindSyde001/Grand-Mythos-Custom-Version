using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Characters.StatusHandler;
using UnityEngine;
using Conditions;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.SceneManagement;

public class BattleStateMachine : MonoBehaviour
{
    private static BattleStateMachine _instance = null!;

    public required Team PlayerTeam;

    public required AnimationClip Intro, Outro;

    public required BattleUIOperation UIOperation;
    
    public required BattleResolution BattleResolution;

    public List<Transform> HeroSpawns = new();
    public List<Transform> EnemySpawns = new();

    public required TMP_Text DebugNotificationText;

    [ReadOnly] public List<BattleCharacterController> PartyLineup = new();
    [ReadOnly] public List<BattleCharacterController> Units = new();

    public readonly SortedList<double, BattleCharacterController> Queue = new(new TimestampedQueueComparer());

    private CancellationTokenSource _targetStateChanged = new();
    private TargetState _targetState, _currentState;
    private bool _skipBattleEndScreen = true;
    private double _timestamp = 0;
    private TaskCompletionSource<bool> _finishedTcs = new();

    public Task<bool> Finished => _finishedTcs.Task;

    public Tactics? TacticsPlaying { get; private set; }

    public BattleCharacterController? UnitPlaying { get; private set; }

    // UPDATES
    private void Awake()
    {
        if (_instance == null!)
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

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null!;
        InputManager.PopGameState(this);
    }

    private void Start()
    {
        Run(destroyCancellationToken).Forget();
    }

    private float DelayScalar(IAction.Delay delay, float baseValue)
    {
        return delay switch
        {
            IAction.Delay.Short => baseValue * 0.75f,
            IAction.Delay.Base => baseValue,
            IAction.Delay.Long => baseValue * 1.25f,
            _ => throw new ArgumentOutOfRangeException(nameof(delay), delay, null)
        };
    }

    private async UniTask Run(CancellationToken cancellation)
    {
        foreach (var target in FindObjectsOfType<BattleCharacterController>())
            Include(target);

        if (Intro)
        {
            BattleCamera.Instance.PlayUninterruptible(Intro);
            await UniTask.Delay(TimeSpan.FromSeconds(Intro.length), cancellationToken: cancellation);
        }

        // Sort them in the order they are setup in the party
        PartyLineup = GameManager.Instance.PartyLineup.Select(x => PartyLineup.FirstOrDefault(y => y.Profile == x)).Where(x => x != null).ToList();

        _targetStateChanged = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        
        bool win;
        while (IsBattleFinished(out win) == false && _targetState != TargetState.End)
        {
            cancellation.ThrowIfCancellationRequested();

            if (_targetState != _currentState)
            {
                _targetStateChanged = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
                _currentState = _targetState;
            }

            if (_currentState == TargetState.Paused)
            {
                await UniTask.NextFrame(cancellation, cancelImmediately: true);
                continue;
            }

            BattleCharacterController? unit = null;
            for (int i = 0; i < Queue.Count; i++)
            {
                if (Queue.Values[i].Profile.CurrentHP == 0)
                    continue;

                _timestamp = Queue.Keys[i];
                unit = Queue.Values[i];
                for (int j = 0; j < i; j++)
                    Queue.RemoveAt(0); // Remove all previous units
            }

            if (unit == null)
            {
                await UniTask.NextFrame(cancellation, cancelImmediately: true);
                continue;
            }

            Tactics? chosenTactic;
            TargetCollection selectionAsTargetCollection;
            if (unit.Profile.Team == PlayerTeam)
            {
                using (Units.TemporaryCopy(out var unitsCopy))
                {
                    if (unit.Profile.Modifiers.FirstOrDefault(x => x.Modifier is TauntModifier) is { Modifier: not null } taunt)
                        unitsCopy.RemoveAll(x => x.Profile != taunt.Source);

                    try
                    {
                        chosenTactic = await UIOperation.RunUIFor(unit, unitsCopy, _targetStateChanged.Token);
                    }
                    catch (Exception e) when (e is OperationCanceledException or TaskCanceledException) // May be interrupted as a result of a state change
                    {
                        _targetStateChanged = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
                        continue;
                    }

                    var allUnits = new TargetCollection(unitsCopy);
                    if (chosenTactic.Condition.CanExecute(chosenTactic.Action, allUnits, unit.Context, out selectionAsTargetCollection) == false)
                        chosenTactic = null;
                }
            }
            else
            {
                chosenTactic = null;
                selectionAsTargetCollection = default;
                using (Units.TemporaryCopy(out var unitsCopy))
                {
                    if (unit.Profile.Modifiers.FirstOrDefault(x => x.Modifier is TauntModifier) is { Modifier: not null } taunt)
                        unitsCopy.RemoveAll(x => x.Profile != taunt.Source);

                    foreach (var tactic in unit.Profile.Tactics)
                    {
                        var allUnits = new TargetCollection(unitsCopy);
                        if (tactic != null && tactic.IsOn && tactic.Condition.CanExecute(tactic.Action, allUnits, unit.Context, out selectionAsTargetCollection))
                        {
                            chosenTactic = tactic;
                            break;
                        }
                    }
                }
            }

            unit.Context.CombatTimestamp = _timestamp;
            unit.Context.Round++;

            IAction.Delay delay;
            if (chosenTactic == null)
            {
                Debug.LogError($"{unit} did not find any tactics to run");
                delay = IAction.Delay.Base;
            }
            else
            {
                try
                {
                    TacticsPlaying = chosenTactic;
                    UnitPlaying = unit;
                    await ProcessUnit(unit, chosenTactic, selectionAsTargetCollection.ToList(), cancellation);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    TacticsPlaying = null;
                    UnitPlaying = unit;
                }

                delay = chosenTactic.Action.DelayToNextTurn;
            }

            var delayDuration = DelayScalar(delay, unit.Profile.ActionRechargeSpeed);

            var unitIndex = Queue.Values.IndexOf(unit);
            if (unitIndex != -1)
                Queue.RemoveAt(unitIndex);

            Queue.Add(_timestamp + delayDuration, unit);
            if (unit.Profile.InFlowState)
            {
                unit.Profile.CurrentFlow -= delayDuration * SingletonManager.Instance.Formulas.FlowDepletionRate;
                if (unit.Profile.CurrentFlow <= 0f)
                {
                    unit.Profile.InFlowState = false;
                    unit.Profile.CurrentFlow = 0f;
                }
            }

            for (int i = unit.Profile.Modifiers.Count - 1; i >= 0; i--)
            {
                var m = unit.Profile.Modifiers[i];
                if (m.Modifier.IsStillValid(m, unit.Context) == false)
                    unit.Profile.Modifiers.RemoveAt(i);
            }
        }

        if (_skipBattleEndScreen == false)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cancellation);
        
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
                await UniTask.Delay(TimeSpan.FromSeconds(Outro.length), cancellationToken: cancellation);
                BattleCamera.Instance.enabled = false;
            }

            await BattleResolution.ResolveBattle(win, this, cancellation);
        }

        foreach (var hero in PartyLineup)
            hero.Profile.CurrentHP = hero.Profile.CurrentHP == 0 ? 1 : hero.Profile.CurrentHP;

        if (gameObject.scene == SceneManager.GetActiveScene())
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i) != gameObject.scene)
                    SceneManager.SetActiveScene(SceneManager.GetSceneAt(i));
            }
        }

        await SceneManager.UnloadSceneAsync(gameObject.scene)!.ToUniTask(cancellationToken: CancellationToken.None);

        _currentState = TargetState.End;
        _finishedTcs.SetResult(win);
    }

    private bool IsBattleFinished(out bool win)
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

    private async UniTask ProcessUnit(BattleCharacterController unit, Tactics chosenTactic, List<BattleCharacterController> preselection, CancellationToken cancellation)
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

            if (chosenTactic.Action.CameraAnimation != null)
                BattleCamera.Instance.TryPlayAnimation(unit, chosenTactic.Action.CameraAnimation);


            await animation.Play(chosenTactic.Action, unit, preselection.ToArray(), cancellation);

            await ApplyEffects();

            async UniTask ApplyEffects()
            {
                using (Units.TemporaryCopy(out var unitsCopy))
                {
                    var allUnits = new TargetCollection(unitsCopy);
                    // Check AGAIN that our selection is still valid, may not be after playing the animation
                    if (chosenTactic.Condition.CanExecute(chosenTactic.Action, allUnits, unit.Context, out var selection) == false)
                    {
                        if (PlayerTeam != unit.Profile.Team)
                            return;

                        // Log to screen/user what went wrong here by re-evaluating with the tracker connected
                        try
                        {
                            var failureTracker = new FailureTracker(chosenTactic.Action);
                            unit.Context.Tracker = failureTracker;
                            chosenTactic.Condition.CanExecute(chosenTactic.Action, allUnits, unit.Context, out _);
                            ShowFailureReason(failureTracker.FailureMessage).Forget();

                            async UniTask ShowFailureReason(string text)
                            {
                                DebugNotificationText.gameObject.SetActive(true);
                                DebugNotificationText.text = text;
                                await UniTask.Delay(TimeSpan.FromSeconds(10f), cancellationToken: cancellation);
                                DebugNotificationText.gameObject.SetActive(false);
                            }
                        }
                        finally
                        {
                            unit.Context.Tracker = null;
                        }

                        return;
                    }

                    var selectionArray = selection.ToArray();
                    var initialHP = new int[selectionArray.Length];
                    for (var i = 0; i < selectionArray.Length; i++)
                        initialHP[i] = selectionArray[i].Profile.CurrentHP;

                    chosenTactic.Action.Perform(selectionArray, unit.Context);

                    chosenTactic.Condition.TargetFilter?.NotifyUsedCondition(selection, unit.Context);
                    chosenTactic.Condition.AdditionalCondition?.NotifyUsedCondition(selection, unit.Context);
                    chosenTactic.Action.TargetFilter?.NotifyUsedCondition(selection, unit.Context);
                    chosenTactic.Action.Precondition?.NotifyUsedCondition(selection, unit.Context);

                    var tasks = new List<UniTask>();
                    for (var i = 0; i < selectionArray.Length; i++)
                    {
                        var controller = selectionArray[i];
                        IActionAnimation? anim;
                        if (controller.Profile.CurrentHP == initialHP[i])
                            anim = controller.Profile.Dodge ?? controller.Profile.Parry ?? controller.Profile.Shield;
                        else
                            anim = controller.Profile.CurrentHP > 0 ? controller.Profile.Hurt : controller.Profile.Death;

                        if (anim is null)
                            continue;

                        var reactionAnimation = anim.Play(null, controller, Array.Empty<BattleCharacterController>(), cancellation);
                        tasks.Add(reactionAnimation);
                    }

                    await UniTask.WhenAll(tasks);
                }
            }
        }
        finally
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.UnlockReloadAssemblies();
#endif
        }
    }

    public static bool TryGetInstance([MaybeNullWhen(false)] out BattleStateMachine bts)
    {
        if (_instance != null!)
        {
            bts = _instance;
            return true;
        }

        bts = null;
        return false;
    }

    public UniTask ForceEnd(bool skipBattleEndScreen)
    {
        _targetState = TargetState.End;
        _targetStateChanged.Cancel();
        _skipBattleEndScreen = skipBattleEndScreen;
        return Finished.AsUniTask();
    }

    public async UniTask Pause(CancellationToken cancellationToken)
    {
        if (_targetState == TargetState.Running)
        {
            _targetState = TargetState.Paused;
            _targetStateChanged.Cancel();
        }
        while (_currentState == TargetState.Running)
            await UniTask.NextFrame(cancellationToken, true);
    }

    public async UniTask Unpause(CancellationToken cancellationToken)
    {
        if (_targetState == TargetState.Paused)
        {
            _targetState = TargetState.Running;
            _targetStateChanged.Cancel();
        }

        while (_currentState == TargetState.Paused)
            await UniTask.NextFrame(cancellationToken, true);
    }

    public async UniTask WaitForSkillPlayed(Skill skill, CancellationToken cancellationToken)
    {
        while (Finished.IsCompleted == false)
            await UniTask.NextFrame(cancellationToken, true);
    }

    public void Include(BattleCharacterController unit)
    {
        if (unit == null)
            throw new NullReferenceException(nameof(unit));

        if (Units.Contains(unit) == false)
        {
            Units.Add(unit);
            Queue.Add(_timestamp + unit.Profile.ActionRechargeSpeed, unit);
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

    public void Exclude(BattleCharacterController? unit)
    {
        if (unit == null)
            return;
        Units.Remove(unit);
        PartyLineup.Remove(unit);
        for (int i = Queue.Count - 1; i >= 0; i--)
        {
            if (Queue.Values[i] == unit)
                Queue.RemoveAt(i);
        }
    }

    private enum TargetState
    {
        Running,
        Paused,
        End
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1, 0.25f);
        foreach (var spawn in HeroSpawns)
            Gizmos.DrawCube(spawn.position + Vector3.up, new Vector3(1, 2, 1));
        foreach (var spawn in EnemySpawns)
            Gizmos.DrawCube(spawn.position + Vector3.up, new Vector3(1, 2, 1));
    }

    private class FailureTracker : IConditionEvalTracker
    {
        private int _stackDepth;
        private int _failureDepth = 0;
        public string FailureMessage = "";
        private IAction _associatedAction;

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

        public void PostActionPrecondition(CharacterTemplate source, IAction action, TargetCollection allTargets) { }

        public void PostActionTargetFilter(CharacterTemplate source, IAction action, TargetCollection previousTargets) { }

        public void PostTargetFilter(CharacterTemplate source, Condition targetFilter) { }

        public void PostAdditionalCondition(CharacterTemplate source, Condition condition, TargetCollection previousTargets) { }

        public void PostSuccess(CharacterTemplate source, TargetCollection previousTargets) { }
    }

    private class TimestampedQueueComparer : IComparer<double>
    {
        public int Compare(double x, double y)
        {
            var v = x.CompareTo(y);
            return v == 0 ? 1 : v;
        }
    }
}

public partial class Settings
{
    public ModifiersBehavior ModifiersBehavior = ModifiersBehavior.RemovedAfterBattle;
}

public enum ModifiersBehavior
{
    RemovedAfterBattle,
    CarriedBetweenBattles
}