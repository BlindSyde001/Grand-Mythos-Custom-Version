using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using StatusHandler;

namespace Characters.Special
{
    [Serializable]
    public class ElementalAmmoSpecial : ISpecial
    {
        [Required] public ElementalAmmo AssociatedStatus;

        public string ButtonLabel => "Elemental Ammo";

        public async UniTask<Tactics?> OnButtonClicked(BattleCharacterController character, IDisposableMenuProvider menuProvider, Func<BattleCharacterController, IAction, CancellationToken, UniTask<Tactics?>> presentTargetUI, CancellationToken cancellation)
        {
            var elementMenu = menuProvider.NewMenuOf<Element?>(nameof(ElementalAmmoSpecial));
            for (Element v = default; v <= Element.Last; v++)
                elementMenu.NewButton(v.ToString(), v);

            var t = await elementMenu.SelectedItem(cancellation);
            if (t is { } selectedValue)
            {
                if (character.Profile.Modifiers.FirstOrDefault(x => x.Modifier is ElementalAmmo) is not {Modifier: ElementalAmmo ammo})
                    character.Profile.Modifiers.Add(new AppliedModifier(character.Context, ammo = UnityEngine.Object.Instantiate(AssociatedStatus), null));

                ammo.Element = selectedValue;
            }

            return null;
        }

        public void OnBattleStart(BattleCharacterController character)
        {
            if (character.Profile.Modifiers.FirstOrDefault(x => x.Modifier is ElementalAmmo) is not {Modifier: ElementalAmmo})
                character.Profile.Modifiers.Add(new (character.Context, UnityEngine.Object.Instantiate(AssociatedStatus), null));
        }
    }
}