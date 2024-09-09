using System;
using System.Collections;
using UnityEngine.InputSystem;

namespace Characters.Special
{
    public interface ISpecial
    {
        string ButtonLabel { get; }
        void OnBattleStart(BattleCharacterController character);
        IEnumerable OnButtonClicked(BattleCharacterController character, IDisposableMenuProvider menuProvider, Func<IAction, IEnumerable> presentTargetUI);
    }
}