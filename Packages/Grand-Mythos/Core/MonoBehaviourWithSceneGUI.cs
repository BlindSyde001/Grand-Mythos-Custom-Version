using System;
using UnityEngine;

public abstract class MonoBehaviourWithSceneGUI : MonoBehaviour
{
    protected abstract void DuringSceneGui(SceneGUIProxy sceneGUI);

#if UNITY_EDITOR
    [UnityEditor.CustomEditor( typeof( MonoBehaviourWithSceneGUI ), true )]
    public class DrawLineEditor : Sirenix.OdinInspector.Editor.OdinEditor
    {
        void OnSceneGUI()
        {
            if (target is MonoBehaviourWithSceneGUI s)
            {
                try
                {
                    s.DuringSceneGui(SceneGUIProxy.Instance);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
#endif
}
