using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Editor
{
    [InitializeOnLoad]
    static class InspectorIcons
    {
        static Dictionary<int, Texture> _objIdToTexture = new();
        static List<Component> _utility = new();

        static InspectorIcons()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;

            ObjectFactory.componentWasAdded += c => _objIdToTexture.Remove(c.gameObject.GetInstanceID());
        }

        static void OnHierarchyGUI(int id, Rect rect)
        {
            if (_objIdToTexture.TryGetValue(id, out var v) == false)
            {
                var go = EditorUtility.InstanceIDToObject(id) as GameObject;
                if (TryGetIcon(go, out v) == false)
                {
                    _objIdToTexture.Add(id, null);
                    return;
                }

                _objIdToTexture.Add(id, v);
            }

            if (v == null)
                return;

            rect.xMax = rect.xMin + rect.height;
            var previousColor = GUI.color;
            GUI.color = new Color(56f / 255, 56f / 255, 56f / 255, 0.9f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previousColor;
            GUI.DrawTexture(rect, v);
        }

        static bool TryGetIcon(GameObject go, out Texture t)
        {
            try
            {
                t = null;
                if (go == null)
                    return false;

                _utility.Clear();
                go.GetComponents(_utility);
                Component mainComponent = _utility.Count > 1 ? _utility[1] : null; // First component is the transform, ignore it
                if (mainComponent is CanvasRenderer && _utility.Count > 2)
                    mainComponent = _utility[2];

                if (mainComponent == null)
                    return false; // Null can also occur with missing prefabs in a scene

                t = EditorGUIUtility.GetIconForObject(mainComponent);
                t ??= EditorGUIUtility.ObjectContent(null, mainComponent.GetType())?.image;
                return t != null;
            }
            finally
            {
                _utility.Clear();
            }
        }
    }
}