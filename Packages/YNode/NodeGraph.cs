using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YNode
{
    /// <summary> Base class for all node graphs </summary>
    [Serializable]
    public abstract class NodeGraph : ScriptableObject
    {
        [SerializeField, SerializeReference, ShowInInspector]
        public List<INodeValue> Nodes = new();

        /// <summary> Add a node to the graph by type </summary>
        public virtual INodeValue AddNode(Type type)
        {
            var value = (INodeValue)Activator.CreateInstance(type);
            Nodes.Add(value);
            return value;
        }

        #if UNITY_EDITOR
        /// <summary> Creates a copy of the original node in the graph </summary>
        public virtual INodeValue CopyNode(INodeValue original)
        {
            var duplicateValue = CloneNodeValue(original);
            Nodes.Add(duplicateValue);
            return duplicateValue;

            static INodeValue CloneNodeValue(INodeValue value)
            {
                var sourceDummy = CreateInstance<ZDummy>();
                sourceDummy.Value = value;
                var newDummy = Instantiate(sourceDummy);
                var srcSerialized = new UnityEditor.SerializedObject(sourceDummy);
                var targetSerialized = new UnityEditor.SerializedObject(newDummy);
                var e = srcSerialized.GetIterator();
                e.Next(true);
                bool enterChildren;
                // Instantiating will clone referenced INodeValue, we'll fix this by re-assigning them to the source object's reference
                do
                {
                    enterChildren = true;
                    if (e.propertyType == UnityEditor.SerializedPropertyType.ManagedReference &&
                        e.managedReferenceValue is INodeValue nValue && e.propertyPath != nameof(ZDummy.Value))
                    {
                        targetSerialized.FindProperty(e.propertyPath).managedReferenceValue = nValue;
                        enterChildren = false;
                    }
                } while (e.Next(enterChildren));

                targetSerialized.ApplyModifiedPropertiesWithoutUndo();

                return newDummy.Value!;
            }
        }
        #endif

        /// <summary> Safely remove a node and all its connections </summary>
        /// <param name="node"> The node to remove </param>
        public virtual void RemoveNode(INodeValue node)
        {
            Nodes.Remove(node);
        }

        /// <summary> Remove all nodes and connections from the graph </summary>
        public virtual void Clear()
        {
            Nodes.Clear();
        }

        protected virtual void OnDestroy()
        {
            // Remove all nodes prior to graph destruction
            Clear();
        }
    }
}
