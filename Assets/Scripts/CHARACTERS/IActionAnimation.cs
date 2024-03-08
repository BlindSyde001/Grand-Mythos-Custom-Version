using System.Collections;
using System.Diagnostics.CodeAnalysis;

public interface IActionAnimation
{
    IEnumerable Play(IAction action, BattleCharacterController controller, BattleCharacterController[] targets);
    bool Validate([MaybeNull]IAction action, CharacterTemplate template, ref string message);
}