using System.Collections.Generic;
using System.IO;
using Screenplay.Nodes.TrackItems;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Source.Screenplay.Editor
{
    public class AnimationTrackItemEditor : OdinValueDrawer<AnimationTrackItem>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            EditorGUILayout.BeginHorizontal();
            var trackItem = ValueEntry.SmartValue;
            GUILayout.Label("Clip");
            if (GUILayout.Button("New"))
            {
                SaveAsset(trackItem, null, (Object)Property.Tree.WeakTargets[0]);
            }
            else if (trackItem.Clip != null && GUILayout.Button("Clone"))
            {
                SaveAsset(trackItem, trackItem.Clip, (Object)Property.Tree.WeakTargets[0]);
            }
            else if (trackItem.Clip != null && trackItem.Target.TryGet(out var target, out _) && GUILayout.Button("Open"))
            {
                var provider = target.GetComponent<TemporaryAnimationProvider>();
                if (provider == null)
                    provider = target.AddComponent<TemporaryAnimationProvider>();
                provider.Clip.Add(trackItem.Clip);
                provider.hideFlags = HideFlags.HideAndDontSave;

                Selection.activeObject = target;
                var animationWindow = EditorWindow.GetWindow<AnimationWindow>();
                animationWindow.animationClip = trackItem.Clip;
            }
            EditorGUILayout.BeginVertical();
            CallNextDrawer(label);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            static void SaveAsset(AnimationTrackItem trackItem, AnimationClip? baseToDuplicate, Object parentObj)
            {
                string defaultName = baseToDuplicate == null ? "New Clip" : $"{baseToDuplicate.name} (2)";
                string? path = AssetDatabase.GetAssetPath(trackItem.Clip);
                path = string.IsNullOrEmpty(path) ? Application.dataPath : Path.GetDirectoryName(path);
                path = EditorUtility.SaveFilePanel("New Clip", path, defaultName, "anim");
                if (string.IsNullOrEmpty(path) == false && path.StartsWith(Application.dataPath))
                {
                    var newClip = baseToDuplicate == null ? new AnimationClip() : Object.Instantiate(baseToDuplicate);
                    path = Path.GetRelativePath(Path.GetDirectoryName(Application.dataPath), path);

                    AssetDatabase.CreateAsset(newClip, path);
                    AssetDatabase.SaveAssets();

                    if (parentObj is YNode.Editor.NodeEditor nodeEditor)
                        parentObj = nodeEditor.Graph;

                    Undo.RecordObject(parentObj, "Create new clip for Track Item");
                    trackItem.Clip = newClip;
                }
            }
        }

        public class TemporaryAnimationProvider : MonoBehaviour, IAnimationClipSource
        {
            public List<AnimationClip> Clip = new();
            public void GetAnimationClips(List<AnimationClip> results) => results.AddRange(Clip);
        }
    }
}
