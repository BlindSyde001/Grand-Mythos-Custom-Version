using System;
using Characters;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class ModifierUnlock : IUnlock
{
    [SerializeReference, InlineProperty]
    public IModifier Modifier;

    public void OnUnlock(HeroExtension hero)
    {
        AppliedModifier mod = default;
        mod.CreationTimeStamp = 0d;
        mod.Modifier = Modifier;
        hero.Modifiers.Add(mod);
    }

    public void OnLock(HeroExtension hero) { }
}