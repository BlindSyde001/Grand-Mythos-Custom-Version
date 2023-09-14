using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFocus : ReloadableBehaviour
{
    public static List<CameraFocus> Queue = new();
    public static CameraFocus CurrentFocus => _currentFocus;
    static CameraFocus _previousFocus, _currentFocus;
    static Coroutine _coroutine;

    [SerializeReference]
    public ICameraControl Control = new OrbitCamera();

    public ICameraTransition FadeIn;
    public ICameraTransition FadeOut;

    protected override void OnEnabled(bool afterDomainReload)
    {
        if (afterDomainReload == false)
            Queue.Add(this);

    }

    protected override void OnDisabled(bool beforeDomainReload)
    {
        if(beforeDomainReload == false)
            Queue.Remove(this);
    }

    void Update()
    {
        if (CurrentFocus != this && Queue[^1] == this && _coroutine == null)
        {
            // Start transition to this one
            _coroutine = StartCoroutine(FocusTransition(CurrentFocus));
        }

        if (CurrentFocus != this)
            return;

        Control.Update(Camera.main, this);
    }

    void OnDrawGizmos()
    {
        GizmosHelper.Label(transform.position, nameof(CameraFocus));
        Control.OnDrawGizmos(this);
    }

    void OnValidate()
    {
        Control?.OnValidate(this);
    }

    IEnumerator FocusTransition(CameraFocus previous)
    {
        if (previous != null && previous.FadeOut != null)
        {
            foreach (var yield in previous.FadeOut.Transition())
                yield return yield;
        }
        if (FadeIn != null)
        {
            foreach (var yield in FadeIn.Transition())
                yield return yield;
        }

        _currentFocus = this;
        _coroutine = null;
    }

    static CameraFocus()
    {
        DomainReloadHelper.BeforeReload += helper => helper.FocusQueue = Queue;
        DomainReloadHelper.AfterReload += helper => Queue.AddRange(helper.FocusQueue);
    }
}

public interface ICameraControl
{
    void Update(Camera camera, CameraFocus focus);
    void OnValidate(CameraFocus focus);
    void OnDrawGizmos(CameraFocus focus);
}

public interface ICameraTransition
{
    IEnumerable Transition();
}

public partial class DomainReloadHelper
{
    public List<CameraFocus> FocusQueue;
}