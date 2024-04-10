using UnityEngine.InputSystem;

public static class InputExtension
{
    public static bool WasPerformedThisFrameUnique(this InputAction action)
    {
        if (action.WasPerformedThisFrame())
        {
            action.Reset();
            return true;
        }

        return false;
    }
}