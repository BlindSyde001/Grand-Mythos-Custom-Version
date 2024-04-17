using System;
using System.Linq;
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

    void OnValidate()
    {
        for (int i = Requirements.Length - 1; i >= 0; i--)
        {
            if (ReferenceEquals(Requirements[i], this))
            {
                Requirements = Requirements[..(i)].Concat(Requirements[(i+1)..]).ToArray();
                i = Math.Min(i, Requirements.Length - 1);
                Debug.LogWarning("Requirements to unlock a node cannot contain the node itself, automatically removed self reference ...");
            }
        }
    }
}