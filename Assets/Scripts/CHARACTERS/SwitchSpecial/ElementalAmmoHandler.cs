using System;
using System.Linq;
using Sirenix.OdinInspector;
using StatusHandler;
using UnityEngine;
using UnityEngine.UI;

namespace SwitchSpecial
{
    [Serializable]
    public class ElementalAmmoHandler : ISwitchSpecialHandler
    {
        [Required] public ElementalAmmo AssociatedStatus;

        public void OnBattleStart(BattleCharacterController character)
        {
            if (character.Profile.Modifiers.FirstOrDefault(x => x is ElementalAmmo) is not ElementalAmmo ammo)
                character.Profile.Modifiers.Add(ammo = UnityEngine.Object.Instantiate(AssociatedStatus));
        }

        public void OnSwitch(BattleCharacterController character, ref Color color)
        {
            if (character.Profile.Modifiers.FirstOrDefault(x => x is ElementalAmmo) is not ElementalAmmo ammo)
                character.Profile.Modifiers.Add(ammo = UnityEngine.Object.Instantiate(AssociatedStatus));

            if (ammo.Element == Element.Last)
                ammo.Element = Element.First;
            else
                ammo.Element += 1;

            color = ammo.Element.GetAssociatedColor();
        }

        public void OnSelectedUnitChanged(BattleCharacterController character, ref Color color)
        {
            if (character.Profile.Modifiers.FirstOrDefault(x => x is ElementalAmmo) is ElementalAmmo ammo)
                color = ammo.Element.GetAssociatedColor();
        }
    }
}