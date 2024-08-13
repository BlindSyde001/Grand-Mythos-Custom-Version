using Sirenix.OdinInspector;
using StatusHandler;
using TMPro;
using UnityEngine.UI;

namespace SwitchSpecial
{
    public class StatusModifierDisplay : ModifierDisplay
    {
        public Image Icon;
        public TMP_Text Text;
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

        void Update()
        {
            Icon.sprite = Modifier.Icon;
            Text.text = Modifier.name;
        }
    }
}