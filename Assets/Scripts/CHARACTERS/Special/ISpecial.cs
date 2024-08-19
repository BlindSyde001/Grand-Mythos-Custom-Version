using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Characters.Special
{
    public interface ISpecial
    {
        string ButtonLabel { get; }
        void OnBattleStart(BattleCharacterController character);
        IEnumerable OnButtonClicked(BattleCharacterController character, InputAction cancelInput, ISpecialButtonProvider submenu, System.Func<IAction, IEnumerable> presentTargetUI);
    }

    public interface ISpecialButtonProvider
    {
        public void SetButtons(IEnumerable<(string Label, System.Action OnClick)> buttons);
    }
}