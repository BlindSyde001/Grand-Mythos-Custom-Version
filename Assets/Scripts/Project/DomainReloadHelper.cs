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

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    static void RunOnStart()
    {
        // We need a list of all the reloadables that existed before domain reload, query unity to build one (PreReload),
        // then store that list into a new game object, finally fetch the gameobject after domain reload (PostReload)
        // to call the reload function over each item of that list
        AssemblyReloadEvents.beforeAssemblyReload += PreReload;
        PostReload();
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
#endif
}