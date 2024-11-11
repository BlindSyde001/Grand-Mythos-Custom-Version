using System;
using System.Diagnostics.CodeAnalysis;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Screenplay
{
    [Serializable, InlineProperty]
    public struct SceneObjectReference<T> where T : Object
    {
        public string ScenePath => _genericRef.ScenePath;
        [SerializeField, HideInInspector] private GenericSceneObjectReference _genericRef;

        public GenericSceneObjectReference ToGeneric => _genericRef;

#if UNITY_EDITOR
        public SceneObjectReference(GameObject obj)
        {
            _genericRef = new(obj);
        }

        public SceneObjectReference(UnityEngine.Component obj)
        {
            _genericRef = new(obj);
        }
#endif

        public bool Empty() => _genericRef.Empty();

        public bool TryGet([MaybeNullWhen(false)] out T obj, out ReferenceState referenceState)
        {
            if (_genericRef.TryGet(out var abstractObj, out referenceState))
            {
                obj = (T)abstractObj;
                return true;
            }

            obj = default;
            return false;
        }

        public static implicit operator GenericSceneObjectReference(SceneObjectReference<T> value)
        {
            return value._genericRef;

        }
    }

    [Serializable]
    public struct GenericSceneObjectReference : ISerializationCallbackReceiver
    {
        public string ScenePath => _scenePath;
        [SerializeField, HideInInspector] private string _scenePath;
        [SerializeField, HideInInspector] private guid _objId;

        public bool Empty() => _objId == default;

        public bool TryGet([MaybeNullWhen(false)] out Object obj, out ReferenceState referenceState)
        {
            if (Empty())
            {
                obj = default;
                referenceState = ReferenceState.Empty;
                return false;
            }

            if (Component.ScreenplayReference.TryGetRef(_objId, out var abstractObj))
            {
                obj = abstractObj;
                referenceState = ReferenceState.Success;
                return true;
            }

            referenceState = ReferenceState.SceneUnloaded;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).path == ScenePath)
                {
                    referenceState = ReferenceState.SceneLoadedButNoRef;
                    break;
                }
            }

            obj = default;
            return false;
        }

#if UNITY_EDITOR
        [SerializeField, HideLabel] private UnityEditor.SceneAsset _sceneAsset;

        public GenericSceneObjectReference(GameObject obj) : this(obj, obj, Component.ScreenplayReference.GetOrCreate(obj))
        {
        }

        public GenericSceneObjectReference(UnityEngine.Component obj) : this(obj, obj.gameObject, Component.ScreenplayReference.GetOrCreate(obj))
        {
        }

        private GenericSceneObjectReference(Object obj, GameObject go, guid objId)
        {
            _scenePath = go.scene.path;
            _sceneAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(_scenePath);
            _objId = objId;
        }
#endif

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            // Make sure the path is always up-to-date
            if (_sceneAsset != null)
                _scenePath = UnityEditor.AssetDatabase.GetAssetPath(_sceneAsset);
#endif
        }

        public void OnAfterDeserialize()
        {
        }

        public bool IsValid()
        {
            return _scenePath != null;
        }
    }

    public enum ReferenceState
    {
        Empty,
        SceneUnloaded,
        SceneLoadedButNoRef,
        Success,
    }
}
