using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StatusHandler;
using UnityEngine.InputSystem;

namespace Characters.Special
{
    [Serializable]
    public class ElementalAmmoSpecial : ISpecial
    {
        [Required] public ElementalAmmo AssociatedStatus;

        public string ButtonLabel => "Elemental Ammo";

        public IEnumerable OnButtonClicked(BattleCharacterController character, InputAction cancelInput, ISpecialButtonProvider submenu, Func<IAction, IEnumerable> presentTargetUI)
        {
            Element chosenElement = default;
            bool chosen = false;
            for (Element v = default; v <= Element.Last; v++)
            {
                var v2 = v;
                submenu.NewButton(v.ToString(), () =>
                {
                    chosenElement = v2;
                    chosen = true;
                }, null);
            }

            while (chosen == false)
            {
                if (cancelInput.WasPerformedThisFrame())
                {
                    submenu.Clear();
                    yield break;
                }

                yield return null;
            }

            if (character.Profile.Modifiers.FirstOrDefault(x => x.Modifier is ElementalAmmo) is not {Modifier: ElementalAmmo ammo})
                character.Profile.Modifiers.Add(new AppliedModifier(character.Context, ammo = UnityEngine.Object.Instantiate(AssociatedStatus), null));

            ammo.Element = chosenElement;
        }

        public void OnBattleStart(BattleCharacterController character)
        {
            if (character.Profile.Modifiers.FirstOrDefault(x => x.Modifier is ElementalAmmo) is not {Modifier: ElementalAmmo})
                character.Profile.Modifiers.Add(new (character.Context, UnityEngine.Object.Instantiate(AssociatedStatus), null));
        }
    }
}