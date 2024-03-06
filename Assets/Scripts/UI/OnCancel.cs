using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class OnCancel : MonoBehaviour
{
    [InfoBox("Will run this action whenever the user press the cancel button. When multiple exist in the scene, the one that was enabled most recently will be processed, and if it is then disabled, the previous to that one, etc.")]
    public UnityEvent Action;

    static int _lastProcess;
    static readonly List<OnCancel> Cancels = new();

    void Update()
    {
        if (Cancels[^1] != this)
            return;

        if (_lastProcess == Time.frameCount)
            return;

        if (EventSystem.current.currentInputModule is not InputSystemUIInputModule module || !module.cancel.action.WasReleasedThisFrame())
            return;

        _lastProcess = Time.frameCount;
        Action.Invoke();
    }

    void OnEnable()
    {
        if (Cancels.Contains(this) == false) // This may happen on domain reload ... I think ? I haven't confirmed
            Cancels.Add(this);
    }
    void OnDisable() => Cancels.Remove(this);

    static OnCancel()
    {
        DomainReloadHelper.BeforeReload += helper => helper.Cancels = Cancels;
        DomainReloadHelper.AfterReload += helper =>
        {
            Cancels.Clear();
            Cancels.AddRange(helper.Cancels);
        };
    }
}

public partial class DomainReloadHelper
{
    public List<OnCancel> Cancels = new();
}