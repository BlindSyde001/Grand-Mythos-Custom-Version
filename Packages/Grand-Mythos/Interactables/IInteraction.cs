using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IInteraction
{
    UniTask InteractEnum(IInteractionSource source, OverworldPlayerController player);
    bool IsValid([MaybeNullWhen(true)] out string error);
    void DuringSceneGui(IInteractionSource source, SceneGUIProxy sceneGUI);
}

public interface IInteractionSource
{
    public Transform transform { get; }
}