using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable, InlineProperty]
public struct SceneReference : ISerializationCallbackReceiver
{
    public string Path => _path;
    [SerializeField, HideInInspector] string _path;

#if UNITY_EDITOR
    [SerializeField, ValidateInput(nameof(SceneNotNull)), HideLabel] UnityEditor.SceneAsset _sceneAsset;

    public SceneReference(UnityEditor.SceneAsset asset)
    {
        _sceneAsset = asset;
        _path = UnityEditor.AssetDatabase.GetAssetPath(asset);
    }

    bool SceneNotNull(UnityEditor.SceneAsset asset, ref string message)
    {
        if (asset == null)
        {
            _path = null;
            message = "Scene must not be null";
            return false;
        }
        _path = UnityEditor.AssetDatabase.GetAssetPath(_sceneAsset);
        bool found = false;
        foreach (var scene in UnityEditor.EditorBuildSettings.scenes)
            if (_path == scene.path)
                found = true;

        if (found == false)
        {
            message = $"Scene {asset.name} is not part of the build";
            return false;
        }

        return true;
    }
#endif

    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        // Make sure the path is always updated
        if (_sceneAsset != null)
            _path = UnityEditor.AssetDatabase.GetAssetPath(_sceneAsset);
#endif
    }

    public void OnAfterDeserialize(){ }

    public bool IsValid()
    {
        return _path != null;
    }
}
