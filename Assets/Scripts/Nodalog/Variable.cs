using UnityEngine;

namespace Nodalog
{
    public abstract class Variable : IdentifiableScriptableObject
    {
        [Multiline(lines:3)] public string Description = "";

#if UNITY_EDITOR
        void OnEnable()
        {
            UnityEditor.EditorApplication.playModeStateChanged += RollbackPlayModeChanges;

            // Rollback changes made in play mode to states
            void RollbackPlayModeChanges(UnityEditor.PlayModeStateChange state)
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                    Resources.UnloadAsset(this);
            }
        }
#endif
    }
}