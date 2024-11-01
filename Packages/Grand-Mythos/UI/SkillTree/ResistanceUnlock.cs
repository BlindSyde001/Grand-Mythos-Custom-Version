using System;

namespace UI.SkillTree
{
    [Serializable]
    public class ResistanceUnlock : IUnlock
    {
        public Element Element = Element.Neutral;
        public ElementalResistance Resistance = ElementalResistance.Neutral;
        public void OnUnlock(HeroExtension hero)
        {
            switch (Element)
            {
                case Element.Neutral:
                    break;
                case Element.Fire:
                    hero.ResistanceFire = Resistance;
                    break;
                case Element.Ice:
                    hero.ResistanceIce = Resistance;
                    break;
                case Element.Lighting:
                    hero.ResistanceLightning = Resistance;
                    break;
                case Element.Water:
                    hero.ResistanceWater = Resistance;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnLock(HeroExtension hero)
        {
            
        }
    }
}