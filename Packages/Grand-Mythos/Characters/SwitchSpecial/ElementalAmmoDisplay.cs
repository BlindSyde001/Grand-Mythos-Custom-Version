using Sirenix.OdinInspector;
using StatusHandler;
using UnityEngine.UI;

namespace SwitchSpecial
{
    public class ElementalAmmoDisplay : ModifierDisplay
    {
        public required Graphic Background;
        [ReadOnly] public ElementalAmmo Modifier = null!;

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