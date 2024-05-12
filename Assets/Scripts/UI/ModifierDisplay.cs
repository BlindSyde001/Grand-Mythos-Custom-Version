using UnityEngine;

public abstract class ModifierDisplay : MonoBehaviour
{
    /// <summary>
    /// When the status is added to the UI, may or may not already be on that character for a while
    /// </summary>
    public abstract void OnDisplayed(BattleCharacterController character, BattleUIOperation battleUI, IModifier modifier);

    /// <summary>
    /// Called after <see cref="OnDisplayed"/> if the modifier was just added to the character
    /// </summary>
    public abstract void OnNewModifier();

    /// <summary>
    /// Called after <see cref="OnDisplayed"/> if the modifier was just removed from the character, this method should destroy/cleanup this display
    /// </summary>
    public abstract void RemoveDisplay();
}