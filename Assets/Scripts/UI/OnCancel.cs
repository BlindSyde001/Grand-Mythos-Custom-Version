#nullable enable
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

[AddComponentMenu(" GrandMythos/UI/OnCancel")]
public class OnCancel : MonoBehaviour
{
    [InfoBox("Will run this action whenever the user press the cancel button. Will only run the one parent to the selection, or none exists, the one that was activated last.")]
    public UnityEvent? Action;

    static int _lastProcess;
    static readonly List<OnCancel> _instances = new();

    void Update()
    {
        if (_instances[^1] == this)
            HandleCancel();
    }

    static void HandleCancel()
    {
        if (_lastProcess == Time.frameCount)
            return;

        if (EventSystem.current.currentInputModule is not InputSystemUIInputModule module)
            return;

        if (module.cancel?.action.WasPerformedThisFrameUnique() == false)
            return;

        var selection = EventSystem.current.currentSelectedGameObject;
        var cancelAsParent = selection == null ? null : selection.GetComponentInParent<OnCancel>();
        if (cancelAsParent != null)
            cancelAsParent.Action?.Invoke();
        else
            _instances[^1].Action?.Invoke();
        _lastProcess = Time.frameCount;
    }

    void OnEnable()
    {
        if (_instances.Contains(this) == false) // This may happen on domain reload ... I think ? I haven't confirmed
            _instances.Add(this);
    }

    void OnDisable() => _instances.Remove(this);

    static OnCancel()
    {
        DomainReloadHelper.BeforeReload += helper => helper.Cancels = _instances;
        DomainReloadHelper.AfterReload += helper =>
        {
            _instances.Clear();
            _instances.AddRange(helper.Cancels);
        };
    }
}

public partial class DomainReloadHelper
{
    public List<OnCancel> Cancels = new();
}