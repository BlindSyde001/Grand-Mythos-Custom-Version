using UnityEngine;

namespace Nodalog
{
    [CreateAssetMenu(menuName = "Nodalog/Flag")]
    public class Flag : ScriptableObject
    {
        public bool State;
        [Multiline(lines:10)] public string Description = "";

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