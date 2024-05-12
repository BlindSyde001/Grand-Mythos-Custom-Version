using UnityEngine;

public class ModifierUnlock : IUnlock
{
    [SerializeReference]
    public IModifier Modifier;

    public void OnUnlock(HeroExtension hero, guid guid)
    {
        hero.SkillModifiers.Add(guid, Modifier);
        hero.Modifiers.Add(Modifier);
    }

    public void OnLock(HeroExtension hero, guid guid)
    {
        if (hero.SkillModifiers.Remove(guid, out var modifier))
            hero.Modifiers.Remove(modifier);
    }
}