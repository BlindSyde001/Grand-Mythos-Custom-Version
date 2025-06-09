using System.Linq;
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

    public static string GetBindingLabel(this InputAction action)
    {
        return string.Join(" or ", action.bindings.Select(x => x.effectivePath[(x.effectivePath.IndexOf('/') + 1)..]));
    }
}