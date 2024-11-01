using Sirenix.OdinInspector;
using StatusHandler;
using UnityEngine.UI;

namespace SwitchSpecial
{
    public class ElementalAmmoDisplay : ModifierDisplay
    {
        public Graphic Background;
        [ReadOnly] public ElementalAmmo Modifier;

        public override void OnDisplayed(CharacterTemplate character, BattleUIOperation battleUI, IModifier modifier)
        {
            Modifier = (ElementalAmmo)modifier;
        }

        public override void OnNewModifier()
        {

        }

        public override void RemoveDisplay()
        {
            Destroy(gameObject);
        }

        void Update()
        {
            Background.color = Modifier.Element.GetAssociatedColor();
        }
    }
}