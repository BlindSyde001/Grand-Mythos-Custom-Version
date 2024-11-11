using Screenplay.Nodes;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Source.Screenplay.Editor
{
    public class TrackPlayerEditor : OdinValueDrawer<TrackPlayer>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            CallNextDrawer(label);
            var track = ValueEntry.SmartValue.Track;
            if (track == null)
                return;

            var player = ValueEntry.SmartValue;

            var values = new int[track.Markers.Length + 1];
            var labels = new string[track.Markers.Length + 1];
            labels[0] = "Start";
            values[0] = -1;
            for (int i = 0; i < track.Markers.Length; i++)
            {
                values[i + 1] = i;
                labels[i + 1] = track.Markers[i].Name ?? "";
            }

            EditorGUILayout.BeginHorizontal();
            {
                player.From = EditorGUILayout.IntPopup(player.From, labels, values);

                labels[0] = "End";
                values[0] = -1;
                player.To = EditorGUILayout.IntPopup(player.To, labels, values);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
