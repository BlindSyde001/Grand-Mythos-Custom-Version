using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using QTE;

public interface IActionAnimation
{
    IAsyncEnumerable<(QTEStart qte, double start, float duration)> Play([CanBeNull]IAction action, BattleCharacterController controller, BattleCharacterController[] targets, CancellationToken cancellation);
    bool Validate([CanBeNull]IAction action, CharacterTemplate template, ref string message);
}