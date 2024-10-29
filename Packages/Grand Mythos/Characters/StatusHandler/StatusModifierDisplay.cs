using Sirenix.OdinInspector;
using StatusHandler;

namespace SwitchSpecial
{
    public class StatusModifierDisplay : ModifierDisplay
    {
        [ReadOnly] public StatusModifier Modifier;

        public override void OnDisplayed(CharacterTemplate character, BattleUIOperation battleUI, IModifier modifier)
        {
            Modifier = (StatusModifier)modifier;
        }

        public override void OnNewModifier()
        {

        }

        public override void RemoveDisplay()
        {
            Destroy(gameObject);
        }
    }
}