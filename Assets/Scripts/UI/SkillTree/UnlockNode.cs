using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UnlockNode : MonoBehaviour
{
    [SerializeReference, Required] public IUnlock Unlock;
    public UnlockNode[] Requirements = Array.Empty<UnlockNode>();

    public Button Button;
    public UnityEvent OnUnlock, OnLock, OnReachable, OnUnreachable;

    protected void OnTransformParentChanged()
    {
        GetComponentInParent<SkillTree>()?.EnsureRegistered(this);
    }
}