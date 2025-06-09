using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Characters.Special
{
    public interface ISpecial
    {
        string ButtonLabel { get; }
        void OnBattleStart(BattleCharacterController character);
        UniTask<Tactics?> OnButtonClicked(BattleCharacterController character, IDisposableMenuProvider menuProvider, Func<BattleCharacterController, IAction, CancellationToken, UniTask<Tactics?>> presentTargetUI, CancellationToken cancellation);
    }
}