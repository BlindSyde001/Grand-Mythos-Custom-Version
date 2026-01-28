using System;
using System.Collections;
using System.Linq;
using Battle;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Random = Unity.Mathematics.Random;

public class BattleCamera : MonoBehaviour
{
    public static BattleCamera Instance;
    [Required] public BattleUIOperation BattleController;
    public Random Random = new(11447788);
    public float TransitionDuration = 2f;
    public float TimeBetweenNewPOV = 10f;

    float _timeBeforeNextCam;
    BattlePointOfView _lastPov;

    DisposableCoroutine? _routine;

    private void Start()
    {
        _timeBeforeNextCam = TimeBetweenNewPOV;
    }

    private void OnEnable()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        Instance = null;
    }

    void Update()
    {
        if (BattleController.BattleManagement.enabled == false)
            return;

        if (_lastPov == null)
            TransitionTo(BattlePointOfView.Instances.First().Value);

        if (Settings.Current.BattleCameraType == CameraType.Dynamic)
        {
            _timeBeforeNextCam -= Time.unscaledDeltaTime;
            if (_timeBeforeNextCam <= 0f)
            {
                _timeBeforeNextCam += TimeBetweenNewPOV;
                TransitionTo(BattlePointOfView.Instances.ElementAt(Random.NextInt(BattlePointOfView.Instances.Count)).Value);
            }
        }
    }

    public void TransitionTo(BattlePointOfView pov)
    {
        if (_lastPov == null)
        {
            _lastPov = pov;
            transform.position = pov.transform.position;
            transform.rotation = pov.transform.rotation;
            return;
        }

        const int priority = 0;

        if (_routine is not null && priority < _routine.Priority)
            return;

        _lastPov = pov;
        _routine?.Dispose();
        _routine = new DisposableCoroutine(this, Transition(), priority);

        IEnumerable Transition()
        {
            try
            {
                Vector3 initPosition = transform.position;
                Quaternion initRotation = transform.rotation;
                for (float f = 0; f < 1f; f += Time.unscaledDeltaTime / TransitionDuration)
                {
                    var fs = Mathf.SmoothStep(0, 1, f);
                    fs = Mathf.SmoothStep(0, 1, fs);
                    fs = Mathf.SmoothStep(0, 1, fs);
                    var p = Vector3.Lerp(initPosition, pov.transform.position, fs);
                    var r = Quaternion.Lerp(initRotation, pov.transform.rotation, fs);
                    transform.SetPositionAndRotation(p, r);
                    yield return null;
                }
            }
            finally
            {
                _routine = null;
            }
        }
    }

    public bool TryPlayAnimation(BattleCharacterController target, AnimationClip clip, float blendIn = 0.25f)
    {
        const int priority = 1;

        if (_routine is not null && priority < _routine.Priority)
            return false;

        _routine?.Dispose();
        _routine = new DisposableCoroutine(this, PlayAnim(), priority);
        return true;

        IEnumerable PlayAnim()
        {
            var graph = PlayableGraph.Create();
            try
            {
                var animator = gameObject.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = gameObject.AddComponent<Animator>();
                    animator.hideFlags |= HideFlags.DontSave | HideFlags.NotEditable;
                }
                graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                var playableClip = AnimationClipPlayable.Create(graph, clip);
                var output = AnimationPlayableOutput.Create(graph, nameof(BattleCamera), animator);
                output.SetSourcePlayable(playableClip);

                var initialPos = transform.position;
                var initialRot = transform.rotation;
                for (float f = 0; f < clip.length; f += Time.unscaledDeltaTime)
                {
                    playableClip.SetTime(f);
                    graph.Evaluate();
                    var animatedPos = transform.position;
                    var animatedRot = transform.rotation;

                    // Animated around the origin, but we want to be relative to target:
                    var centeredPos = target.transform.rotation * animatedPos + target.transform.position;
                    var centeredRot = target.transform.rotation * animatedRot;

                    // Blending in
                    var p = f < blendIn ? Vector3.Lerp(initialPos, centeredPos, f / blendIn) : centeredPos;
                    var r = f < blendIn ? Quaternion.Lerp(initialRot, centeredRot, f / blendIn) : centeredRot;
                    transform.SetPositionAndRotation(p, r);
                    yield return null;
                }

                _routine = null;
                TransitionTo(_lastPov);
            }
            finally
            {
                if (graph.IsValid())
                    graph.Destroy();
                _routine = null;
            }
        }
    }

    public void PlayUninterruptible(AnimationClip clip, bool transitionOut = true, float blendIn = 0f)
    {
        _routine?.Dispose();
        _routine = new DisposableCoroutine(this, PlayAnim(), int.MaxValue);

        IEnumerable PlayAnim()
        {
            var graph = PlayableGraph.Create();
            try
            {
                var animator = gameObject.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = gameObject.AddComponent<Animator>();
                    animator.hideFlags |= HideFlags.DontSave | HideFlags.NotEditable;
                }
                graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                var playableClip = AnimationClipPlayable.Create(graph, clip);
                var output = AnimationPlayableOutput.Create(graph, nameof(BattleCamera), animator);
                output.SetSourcePlayable(playableClip);

                var initialPos = transform.position;
                var initialRot = transform.rotation;
                for (float f = 0; f < clip.length; f += Time.unscaledDeltaTime)
                {
                    playableClip.SetTime(f);
                    graph.Evaluate();
                    var animatedPos = transform.position;
                    var animatedRot = transform.rotation;

                    // Blending in
                    var p = f < blendIn ? Vector3.Lerp(initialPos, animatedPos, f / blendIn) : animatedPos;
                    var r = f < blendIn ? Quaternion.Lerp(initialRot, animatedRot, f / blendIn) : animatedRot;
                    transform.SetPositionAndRotation(p, r);
                    yield return null;
                }

                if (transitionOut)
                {
                    _routine = null;
                    TransitionTo(_lastPov);
                }
            }
            finally
            {
                if (graph.IsValid())
                    graph.Destroy();
                _routine = null;
            }
        }
    }

    public enum CameraType
    {
        Static,
        Dynamic
    }

    public class DisposableCoroutine : IDisposable
    {
        public readonly int Priority;

        IDisposable _disposable;
        Coroutine _routine;
        MonoBehaviour _host;
        
        public DisposableCoroutine(MonoBehaviour host, IEnumerable enumerator, int priority)
        {
            var e = enumerator.GetEnumerator();
            _disposable = (IDisposable)e;
            _routine = host.StartCoroutine(e);
            _host = host;
            Priority = priority;
        }

        public void Dispose()
        {
            _disposable?.Dispose();
            _disposable = null;
            _host.StopCoroutine(_routine);
        }
    }
}

public partial class Settings
{
    public BattleCamera.CameraType BattleCameraType = BattleCamera.CameraType.Static;
}