using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

public interface IActionAnimation
{
    UniTask Play(IAction? action, BattleCharacterController controller, BattleCharacterController[] targets, CancellationToken cancellation);
    bool Validate(IAction? action, CharacterTemplate template, ref string message);
}