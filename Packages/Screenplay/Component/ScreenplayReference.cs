using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Screenplay.Component
{
    [ExecuteAlways]
    public class ScreenplayReference : MonoBehaviour, ISerializationCallbackReceiver
    {
        private static readonly Dictionary<guid, Object> s_idToRef = new();
        private static readonly Dictionary<Object, guid> s_refToId = new();

        [OnValueChanged(nameof(ReAssignReference)), SerializeField]
        private Object? Reference;

        [ReadOnly, SerializeField, DisplayAsString]
        private guid _guid = guid.New();

        public guid Guid => _guid;

        private void ReAssignReference()
        {
            lock (s_idToRef)
            {
                if (s_idToRef.Remove(Guid, out var existingRef))
                {
                    s_refToId.Remove(existingRef);

                    if (Reference is not null)
                    {
                        s_idToRef[Guid] = Reference;
                        s_refToId[Reference] = Guid;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            lock (s_idToRef)
            {
                if (s_idToRef.TryGetValue(Guid, out var existingRef) && ReferenceEquals(existingRef, Reference))
                {
                    s_idToRef.Remove(Guid);
                    s_refToId.Remove(Reference);
                }
            }
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            if (Reference == null)
            {
                Debug.LogWarning("Missing Screenplay Reference", this);
                return;
            }

            lock (s_idToRef)
            {
                if (s_refToId.TryGetValue(Reference, out var existingId))
                {
                    Debug.LogWarning($"'{Guid}' and '{existingId}' refer to the same object, skipping former.", Reference);
                    return;
                }

                while (s_idToRef.TryGetValue(Guid, out var existingRef) && existingRef != null)
                {
                    Debug.LogWarning("Guid collision, assigning a new guid for this object. If you're creating a new object by duplicating an existing one you can ignore this warning. Click on me to select the source object", existingRef);
                    Debug.LogWarning("Click on me to select the conflicting object", Reference);
                    _guid = guid.New();
                }

                s_idToRef[Guid] = Reference;
                s_refToId[Reference] = Guid;
            }
        }

        public static bool TryGetRef(guid guid, out Object obj) => s_idToRef.TryGetValue(guid, out obj) && obj != null; // obj may be destroyed
        public static bool TryGetId(Object obj, out guid guid) => s_refToId.TryGetValue(obj, out guid);

        public static guid GetOrCreate(GameObject obj)
        {
            if (s_refToId.TryGetValue(obj, out var guid))
            {
                return guid;
            }

            var reff = obj.AddComponent<ScreenplayReference>();
            reff.Reference = obj;
            guid = reff.Guid;

            s_idToRef[guid] = obj;
            s_refToId[obj] = guid;
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(obj);
            #endif
            return guid;
        }

        public static guid GetOrCreate(UnityEngine.Component comp)
        {
            if (s_refToId.TryGetValue(comp, out var guid))
            {
                return guid;
            }

            var reff = comp.gameObject.AddComponent<ScreenplayReference>();
            reff.Reference = comp;
            guid = reff.Guid;

            s_idToRef[guid] = comp;
            s_refToId[comp] = guid;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(comp);
#endif
            return guid;
        }
    }
}
