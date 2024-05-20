using UnityEngine;

namespace Nodalog
{
    public abstract class Variable : IdentifiableScriptableObject
    {
        [Multiline(lines:3)] public string Description = "";

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += RollbackPlayModeChanges;

            // Rollback changes made in play mode to states
            void RollbackPlayModeChanges(UnityEditor.PlayModeStateChange state)
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                    Resources.UnloadAsset(this);
            }
#endif
        }

        protected virtual void OnDisable()
        {

        }
    }
}