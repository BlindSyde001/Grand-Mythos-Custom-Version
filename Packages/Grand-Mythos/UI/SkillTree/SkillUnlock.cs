using System;

[Serializable]
public class SkillUnlock : IUnlock
{
    public required Skill Skill;
    public void OnUnlock(HeroExtension hero) => hero.Skills.Add(Skill);
    public void OnLock(HeroExtension hero) => hero.Skills.Remove(Skill);
}