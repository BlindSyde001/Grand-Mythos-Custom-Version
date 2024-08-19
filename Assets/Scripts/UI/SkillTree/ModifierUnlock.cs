using Characters;
using UnityEngine;

public class ModifierUnlock : IUnlock
{
    [SerializeReference]
    public IModifier Modifier;

    public void OnUnlock(HeroExtension hero, guid guid)
    {
        hero.SkillModifiers.Add(guid, Modifier);
        AppliedModifier mod = default;
        mod.CreationTimeStamp = 0d;
        mod.Modifier = Modifier;
        hero.Modifiers.Add(mod);
    }

    public void OnLock(HeroExtension hero, guid guid)
    {
        if (hero.SkillModifiers.Remove(guid, out var modifier))
        {
            hero.Modifiers.RemoveAll(x => x.Modifier == modifier);
        }
    }
}