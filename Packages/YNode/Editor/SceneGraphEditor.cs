using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YNode;

namespace YNode.Editor
{
    [CustomEditor(typeof(SceneGraph), true)]
    public class SceneGraphEditor : UnityEditor.Editor
    {
        private SceneGraph? _sceneGraph;
        private Type? _graphType;
        private bool _removeSafely;
        private SceneGraph SceneGraph => _sceneGraph != null ? _sceneGraph : _sceneGraph = (SceneGraph)target;

        private void OnEnable()
        {
            Type sceneGraphType = SceneGraph.GetType();
            if (sceneGraphType == typeof(SceneGraph))
            {
                _graphType = null;
            }
            else
            {
                Type baseType = sceneGraphType.BaseType!;
                if (baseType.IsGenericType)
                {
                    _graphType = sceneGraphType = baseType.GetGenericArguments()[0];
                }
            }
        }

        public override void OnInspectorGUI()
        {
            if (SceneGraph.graph == null)
            {
                if (GUILayout.Button("New graph", GUILayout.Height(40)))
                {
                    if (_graphType == null)
                    {
                        Type[] graphTypes = NodeEditorReflection.GetDerivedTypes(typeof(NodeGraph));
                        GenericMenu menu = new GenericMenu();
                        for (int i = 0; i < graphTypes.Length; i++)
                        {
                            Type graphType = graphTypes[i];
                            menu.AddItem(new GUIContent(graphType.Name), false, () => CreateGraph(graphType));
                        }

                        menu.ShowAsContext();
                    }
                    else
                    {
                        CreateGraph(_graphType);
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Open graph", GUILayout.Height(40)))
                {
                    GraphWindow.Open(SceneGraph.graph);
                }

                if (_removeSafely)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Really remove graph?");
                    GUI.color = new Color(1, 0.8f, 0.8f);
                    if (GUILayout.Button("Remove"))
                    {
                        _removeSafely = false;
                        Undo.RecordObject(SceneGraph, "Removed graph");
                        SceneGraph.graph = null;
                    }

                    GUI.color = Color.white;
                    if (GUILayout.Button("Cancel"))
                    {
                        _removeSafely = false;
                    }

                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUI.color = new Color(1, 0.8f, 0.8f);
                    if (GUILayout.Button("Remove graph"))
                    {
                        _removeSafely = true;
                    }

                    GUI.color = Color.white;
                }
            }

            DrawDefaultInspector();
        }

        public void CreateGraph(Type type)
        {
            Undo.RecordObject(SceneGraph, "Create graph");
            SceneGraph.graph = (NodeGraph)CreateInstance(type);
            SceneGraph.graph.name = SceneGraph.name + "-graph";
        }
    }
}
