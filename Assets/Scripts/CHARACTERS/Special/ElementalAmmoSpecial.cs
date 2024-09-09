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

        public IEnumerable OnButtonClicked(BattleCharacterController character, IDisposableMenuProvider menuProvider, Func<IAction, IEnumerable> presentTargetUI)
        {
            var elementMenu = menuProvider.NewMenuOf<Element>(nameof(ElementalAmmoSpecial));
            for (Element v = default; v <= Element.Last; v++)
                elementMenu.NewButton(v.ToString(), v);

            var task = elementMenu.SelectedItem();
            while (task.IsCompleted == false)
                yield return null;

            if (task.IsCanceled)
                yield break;

            if (character.Profile.Modifiers.FirstOrDefault(x => x.Modifier is ElementalAmmo) is not {Modifier: ElementalAmmo ammo})
                character.Profile.Modifiers.Add(new AppliedModifier(character.Context, ammo = UnityEngine.Object.Instantiate(AssociatedStatus), null));

            ammo.Element = task.Result;
        }

        public void OnBattleStart(BattleCharacterController character)
        {
            if (character.Profile.Modifiers.FirstOrDefault(x => x.Modifier is ElementalAmmo) is not {Modifier: ElementalAmmo})
                character.Profile.Modifiers.Add(new (character.Context, UnityEngine.Object.Instantiate(AssociatedStatus), null));
        }
    }
}