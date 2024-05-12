using System;
using System.Linq;
using Sirenix.OdinInspector;
using StatusHandler;

namespace SwitchSpecial
{
    [Serializable]
    public class ElementalAmmoHandler : ISwitchSpecialHandler
    {
        [Required] public ElementalAmmo AssociatedStatus;

        public void OnSwitch(BattleCharacterController character)
        {
            if (character.Profile.Modifiers.FirstOrDefault(x => x is ElementalAmmo) is not ElementalAmmo ammo)
                character.Profile.Modifiers.Add(ammo = UnityEngine.Object.Instantiate(AssociatedStatus));

            if (ammo.Element == Element.Last)
                ammo.Element = Element.First;
            else
                ammo.Element += 1;
        }
    }
}