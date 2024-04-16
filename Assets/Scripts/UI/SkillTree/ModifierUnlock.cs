using UnityEngine;

public class ModifierUnlock : IUnlock
{
    [SerializeReference]
    public IModifier Modifier;

    public void OnUnlock(HeroExtension hero, guid guid)
    {
        hero.Modifiers.Add(guid, Modifier);
    }

    public void OnLock(HeroExtension hero, guid guid)
    {
        hero.Modifiers.Remove(guid);
    }
}