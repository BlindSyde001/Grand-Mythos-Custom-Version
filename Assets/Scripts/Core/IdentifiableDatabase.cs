using System;
using System.Collections.Generic;
using UnityEngine;

public class IdentifiableDatabase : ScriptableObject, ISerializationCallbackReceiver
{
    static IdentifiableDatabase __instance;

    static IdentifiableDatabase Instance
    {
        get
        {
            __instance ??= Resources.Load<IdentifiableDatabase>("IdentifiableDatabase");
#if UNITY_EDITOR
            if (__instance == null)
            {
                Debug.LogError("Could not load IdentifiableDatabase resource - automatically creating an instance in 'Resources/'");
                __instance = CreateInstance<IdentifiableDatabase>();
                if (UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources") == false)
                    UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                UnityEditor.AssetDatabase.CreateAsset(__instance, "Assets/Resources/IdentifiableDatabase.asset");
                UnityEditor.AssetDatabase.SaveAssets();
            }
#endif

            return __instance;
        }
    }

    [SerializeField]
    List<IdentifiableScriptableObject> _identifiables = new();
    Dictionary<guid, IdentifiableScriptableObject> _findByGuid;
    bool _cleanupScheduled = false;


    public static bool TryGet<T>(guid guid, out T item) where T : IdentifiableScriptableObject
    {
        if (guid == default)
        {
            item = null;
            return true;
        }

        if (Instance._findByGuid == null)
        {
            Instance._findByGuid = new();
            foreach (var identifiable in Instance._identifiables)
            {
                if (identifiable.Guid == default)
                {
                    Debug.LogException(new InvalidOperationException($"{identifiable} has an unset GUID, this is invalid"), identifiable);
                    continue;
                }

                Instance._findByGuid.Add(identifiable.Guid, identifiable);
            }
        }

        if (Instance._findByGuid.TryGetValue(guid, out var itemTypeless) && itemTypeless is T itemStronglyTyped)
        {
            item = itemStronglyTyped;
            return true;
        }

        item = default;
        return false;
    }

    public static void EnsureRegistered(IdentifiableScriptableObject iso)
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.update += FixupThisIdentifiable;

        void FixupThisIdentifiable()
        {
            UnityEditor.EditorApplication.update -= FixupThisIdentifiable;

            lock (Instance._identifiables)
            {
                if (Instance._cleanupScheduled == false)
                {
                    Instance._cleanupScheduled = true;
                    UnityEditor.EditorApplication.update += Cleanup;
                }

                if (Instance._identifiables.Contains(iso))
                    return;

                Instance._identifiables.Add(iso);
                UnityEditor.EditorUtility.SetDirty(Instance);
            }
        }

        static void Cleanup()
        {
            UnityEditor.EditorApplication.update -= Cleanup;
            lock (Instance._identifiables)
            {
                for (int i = Instance._identifiables.Count - 1; i >= 0; i--)
                {
                    if (Instance._identifiables[i] == null)
                    {
                        Instance._identifiables.RemoveAt(i);
                        UnityEditor.EditorUtility.SetDirty(Instance);
                    }
                }
            }
        }
        #endif
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        for (int i = _identifiables.Count - 1; i >= 0; i--)
        {
            if (_identifiables[i] == null)
            {
                _identifiables.RemoveAt(i);
                UnityEditor.EditorUtility.SetDirty(Instance);
            }
        }
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        for (int i = _identifiables.Count - 1; i >= 0; i--)
        {
            if (_identifiables[i] == null)
                _identifiables.RemoveAt(i);
        }
    }
}