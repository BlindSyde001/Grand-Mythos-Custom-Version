using System;
using Sirenix.OdinInspector;

[Serializable]
public class SkillUnlock : IUnlock
{
    [Required] public Skill Skill;
    public void OnUnlock(HeroExtension hero) => hero.Skills.Add(Skill);
    public void OnLock(HeroExtension hero) => hero.Skills.Remove(Skill);
}