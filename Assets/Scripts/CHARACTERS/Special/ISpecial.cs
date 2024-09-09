using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Characters.Special
{
    public interface ISpecial
    {
        string ButtonLabel { get; }
        void OnBattleStart(BattleCharacterController character);
        IEnumerable OnButtonClicked(BattleCharacterController character, InputAction cancelInput, ISpecialButtonProvider submenu, Func<IAction, IEnumerable> presentTargetUI);
    }

    public interface ISpecialButtonProvider
    {
        Button NewButton(string label, Action onClick, [MaybeNull] Func<string> onHover = null);
        void Clear();
    }
}