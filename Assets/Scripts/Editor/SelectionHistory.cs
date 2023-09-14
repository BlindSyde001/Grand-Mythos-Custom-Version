using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor
{
    [InitializeOnLoad]
    public class SelectionHistory : EditorWindow
    {
        static SelectionHistory instance;

        public List<SelectionStruct> History = new();
        public int Current;
        EditorWindow inspectorWindow;
        Rect lastInspectorWindowRect;

        [Serializable]
        public struct SelectionStruct
        {
            public Object[] Objects;
        }

        static SelectionHistory()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI += InspectorHeaderGUI;
        }

        void OnDisable()
        {
            Selection.selectionChanged -= SelectionChanged;
        }

        void OnEnable()
        {
            if (History.Count == 0)
                History.Add(new SelectionStruct { Objects = Selection.objects });
            Selection.selectionChanged += SelectionChanged;
        }

        static void InspectorHeaderGUI(UnityEditor.Editor obj)
        {
            if (instance == null)
            {
                List<SelectionStruct> History = new();
                int Current = 0;
                while (HasOpenInstances<SelectionHistory>())
                {
                    SelectionHistory window = GetWindow<SelectionHistory>();
                    History = window.History;
                    Current = window.Current;
                    window.Close();
                }

                foreach (SelectionHistory window in FindObjectsOfType<SelectionHistory>())
                {
                    History = window.History;
                    Current = window.Current;
                    window.Close();
                }

                instance = CreateInstance<SelectionHistory>();
                instance.Current = Current;
                instance.History = History;
            }

            instance.hideFlags = HideFlags.DontSave;
            instance.Draw();
        }

        void SelectionChanged()
        {
            Object[] latestSelection = Selection.objects;
            if (latestSelection.Length == 0)
                return; // No reason to store empty selection

            if (History.Count > 0 && latestSelection.SequenceEqual(History[Current].Objects))
                return;

            // If we are in the middle of history, remove any node past the current one to continue from this new selection
            while (History.Count > Current + 1)
                History.RemoveAt(History.Count - 1);

            History.Add(new SelectionStruct { Objects = latestSelection });
            while (History.Count > 64)
                History.RemoveAt(0);
            Current = History.Count - 1;
            Repaint();
        }

        void Draw()
        {
            Rect line = EditorGUILayout.GetControlRect(true, 0f);
            line.height = 16f;
            line.y = 38f;
            line.x -= 4f;

            Rect left = line;
            left.width = 22f;
            Rect right = left;
            right.x += right.width;

            GUI.enabled = Current > 0;
            if (GUI.Button(left, "↶"))
            {
                Current -= 1;
                Selection.objects = History[Current].Objects;
            }

            GUI.enabled = Current + 1 < History.Count;
            if (GUI.Button(right, "↷"))
            {
                Current += 1;
                Selection.objects = History[Current].Objects;
            }
            GUI.enabled = true;
        }
    }
}