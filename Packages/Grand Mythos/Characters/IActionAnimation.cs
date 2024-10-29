using System.Collections;
using JetBrains.Annotations;

public interface IActionAnimation
{
    IEnumerable Play(IAction action, BattleCharacterController controller, BattleCharacterController[] targets);
    bool Validate([CanBeNull]IAction action, CharacterTemplate template, ref string message);
}