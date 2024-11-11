using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Screenplay.Editor
{
    public class SceneObjectReferenceDrawer<T> : OdinValueDrawer<SceneObjectReference<T>> where T : Object
    {
        private static readonly GUIContent s_unloadedContent = new GUIContent("Load ref scene ?", "This reference is not loaded, press this button to swap to its scene");

        protected override void DrawPropertyLayout(GUIContent? label)
        {
            var value = ValueEntry.SmartValue;
            if (Property.GetAttribute<RequiredAttribute>() != null && value.Empty())
                SirenixEditorGUI.ErrorMessageBox($"{Property.NiceName} is required");

            Rect rect = EditorGUILayout.GetControlRect();
            value.TryGet(out T? obj, out var state);
            if (state == ReferenceState.SceneUnloaded)
            {
                if (label != null)
                    rect = EditorGUI.PrefixLabel(rect, label);

                GUIHelper.PushLabelWidth(20);
                if (GUI.Button(rect, s_unloadedContent))
                    EditorSceneManager.OpenScene(value.ScenePath);
                GUIHelper.PopLabelWidth();
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                if (label == null)
                    obj = (T)EditorGUI.ObjectField(rect, obj, typeof(T), true);
                else
                    obj = (T)EditorGUI.ObjectField(rect, label, obj, typeof(T), true);
                if (EditorGUI.EndChangeCheck())
                {
                    if (typeof(UnityEngine.Component).IsAssignableFrom(typeof(T)))
                        this.ValueEntry.SmartValue = new((UnityEngine.Component)(Object)obj);
                    else
                        this.ValueEntry.SmartValue = new((GameObject)(Object)obj);
                }
            }
        }
    }
}
