using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Handles rebuilding un-serialized state back after a domain reload, see <see cref="ReloadableBehaviour"/>.
/// </summary>
public partial class DomainReloadHelper : MonoBehaviour
{
    /// <summary>
    /// Called before the domain is reloaded, so before un-serialized fields are reset, and before components have their OnEnable() called.
    /// </summary>
    public static System.Action<DomainReloadHelper> BeforeReload;
    /// <summary>
    /// Called after the domain is reloaded, so after un-serialized fields are reset, but before components have their OnEnable() called.
    /// </summary>
    public static System.Action<DomainReloadHelper> AfterReload;
    /// <summary>
    /// Called after exiting play mode.
    /// </summary>
    public static System.Action OnEnterEditMode;
    /// <summary>
    /// When the game is not entering or exiting edit mode, and exiting play mode
    /// </summary>
    public static LastPlayModeState LastState = 
#if UNITY_EDITOR
        LastPlayModeState.EnteredEditMode;
#else
        LastPlayModeState.EnteredPlayMode;
#endif

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    static void RunOnStart()
    {
        // We need a list of all the reloadables that existed before domain reload, query unity to build one (PreReload),
        // then store that list into a new game object, finally fetch the gameobject after domain reload (PostReload)
        // to call the reload function over each item of that list
        AssemblyReloadEvents.beforeAssemblyReload += PreReload;
        PostReload();
        EditorApplication.playModeStateChanged += OnEnterPlaymodeInEditor;
    }

    static void PreReload()
    {
        if(EditorApplication.isPlaying == false)
            return;

        var helper = new GameObject(nameof(DomainReloadHelper)).AddComponent<DomainReloadHelper>();
        BeforeReload?.Invoke(helper);
    }

    static void PostReload()
    {
        var reloadHelper = FindAnyObjectByType<DomainReloadHelper>(FindObjectsInactive.Include);
        if (reloadHelper is null)
            return;

        // This occurs before components have their OnEnable() called

        AfterReload?.Invoke(reloadHelper);
        Destroy(reloadHelper);
    }

    static void OnEnterPlaymodeInEditor(PlayModeStateChange change)
    {
        LastState = change switch
        {
            PlayModeStateChange.EnteredEditMode => LastPlayModeState.EnteredEditMode,
            PlayModeStateChange.ExitingEditMode => LastPlayModeState.ExitingEditMode,
            PlayModeStateChange.EnteredPlayMode => LastPlayModeState.EnteredPlayMode,
            PlayModeStateChange.ExitingPlayMode => LastPlayModeState.ExitingPlayMode,
            _ => throw new ArgumentOutOfRangeException(nameof(change), change, null)
        };

        switch (change)
        {
            case PlayModeStateChange.EnteredEditMode:
                OnEnterEditMode?.Invoke();
                break;
            case PlayModeStateChange.ExitingEditMode:
                break;
            case PlayModeStateChange.EnteredPlayMode:
                break;
            case PlayModeStateChange.ExitingPlayMode:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(change), change, null);
        }
    }
#endif

    public enum LastPlayModeState
    {
        /// <summary>
        ///   <para>Occurs during the next update of the Editor application if it is in edit mode and was previously in play mode.</para>
        /// </summary>
        EnteredEditMode,
        /// <summary>
        ///   <para>Occurs when exiting edit mode, before the Editor is in play mode.</para>
        /// </summary>
        ExitingEditMode,
        /// <summary>
        ///   <para>Occurs during the next update of the Editor application if it is in play mode and was previously in edit mode.</para>
        /// </summary>
        EnteredPlayMode,
        /// <summary>
        ///   <para>Occurs when exiting play mode, before the Editor is in edit mode.</para>
        /// </summary>
        ExitingPlayMode,
    }
}