using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace YNode.Editor
{
    /// <summary> Deals with modified assets </summary>
    internal class GraphImporter : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets)
            {
                // Skip processing anything without the .asset extension
                if (Path.GetExtension(path) != ".asset") continue;

                // Get the object that is requested for deletion
                NodeGraph graph = AssetDatabase.LoadAssetAtPath<NodeGraph>(path);
                if (graph == null) continue;

                // Get attributes
                Type graphType = graph.GetType();
                RequireNodeAttribute[] attribs = Array.ConvertAll(
                    graphType.GetCustomAttributes(typeof(RequireNodeAttribute), true),
                    x => (RequireNodeAttribute)x);

                Vector2 position = Vector2.zero;
                foreach (RequireNodeAttribute attrib in attribs)
                {
                    if (attrib.type0 != null) AddRequired(graph, attrib.type0, ref position);
                    if (attrib.type1 != null) AddRequired(graph, attrib.type1, ref position);
                    if (attrib.type2 != null) AddRequired(graph, attrib.type2, ref position);
                }
            }
        }

        private static void AddRequired(NodeGraph graph, Type type, ref Vector2 position)
        {
            if (graph.Nodes.All(x => x.GetType() != type))
            {
                var node = graph.AddNode(type);
                node.Position = position;
                position.x += 200;
            }
        }
    }
}
