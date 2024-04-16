using Sirenix.OdinInspector;

public class SkillUnlock : IUnlock
{
    [Required] public Skill Skill;
    public void OnUnlock(HeroExtension hero, guid guid) => hero.Skills.Add(Skill);
    public void OnLock(HeroExtension hero, guid guid) => hero.Skills.Remove(Skill);
}