using UnityEngine;
using UnityEngine.UI;

namespace SwitchSpecial
{
    public interface ISwitchSpecialHandler
    {
        void OnBattleStart(BattleCharacterController character);
        void OnSwitch(BattleCharacterController character, ref Color color);
        void OnSelectedUnitChanged(BattleCharacterController character, ref Color color);
    }
}