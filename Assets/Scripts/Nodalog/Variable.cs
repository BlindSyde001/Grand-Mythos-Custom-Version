using UnityEngine;

namespace Nodalog
{
    public abstract class Variable : IdentifiableScriptableObject
    {
        [TextArea] public string Description = "";

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += StateChanged;
#endif
            if (Application.isPlaying)
                OnPlay();
        }

        protected virtual void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= StateChanged;
#endif
            if (Application.isPlaying)
                OnExit();
        }

        #if UNITY_EDITOR

        // Rollback changes made in play mode to states
        void StateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.EnteredPlayMode)
                OnPlay();
            else if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                OnExit();
        }
        #endif

        protected abstract void OnPlay();
        protected abstract void OnExit();
    }
}