using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct AnimationState
{
    public string StateName;
    public int Layer;

    public bool EditorOnlyValidate(Animator? animator, out string message, bool logToConsole = true)
    {
        if (animator is null)
        {
            message = $"{nameof(animator)} is null";
            return false;
        }

        if (animator.runtimeAnimatorController == null)
        {
            message = $"{animator}'s controller is null";
            return false;
        }

#if UNITY_EDITOR
        var editTimeController = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(UnityEditor.AssetDatabase.GetAssetPath(animator.runtimeAnimatorController));
        if (Layer >= 0 && Layer < editTimeController.layers.Length)
        {
            foreach (var state in editTimeController.layers[Layer].stateMachine.states)
            {
                if (state.state.name == StateName)
                {
                    message = "";
                    return true;
                }
            }
        }

        List<string> strings = new();
        for (int i = 0; i < editTimeController.layers.Length; i++)
        {
            foreach (var state in editTimeController.layers[i].stateMachine.states)
                strings.Add($"{i}: {state.state.name}");
        }

        message = $"Animator '{animator.runtimeAnimatorController.name}' does not have a state named '{StateName}' on layer {Layer}. States: \n{string.Join(" | ", strings.OrderBy(x => x))}";
        if (logToConsole)
            Debug.LogWarning($"{message}, click on me to navigate to that controller", animator.runtimeAnimatorController);
        return false;
#else
        message = "";
        return true;
#endif
    }
}