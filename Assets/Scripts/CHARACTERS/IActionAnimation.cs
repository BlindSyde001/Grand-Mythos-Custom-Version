using System.Collections;

public interface IActionAnimation
{
    IEnumerable Play(IAction action, BattleCharacterController controller, TargetCollection targets);
    bool Validate(IAction action, CharacterTemplate template, ref string message);
}