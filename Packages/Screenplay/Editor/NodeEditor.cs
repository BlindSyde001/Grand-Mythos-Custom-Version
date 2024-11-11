using UnityEngine;
using YNode.Editor;
using Screenplay.Nodes;
using Event = Screenplay.Nodes.Event;

namespace Screenplay.Editor
{
    public class NodeEditor : CustomNodeEditor<ScreenplayNode>
    {
        [HideInInspector] public bool InPreviewPath, Reachable;

        public override void OnHeaderGUI()
        {
            var thisScreenplay = (ScreenplayGraph)Graph;
            var textColor = GUI.color;
            if (Value is IAction thisAction && Reachable == false)
                GUI.color = new Color(GUI.color.r, GUI.color.g * 0.25f, GUI.color.b * 0.25f, GUI.color.a);
            if (Value is IPrerequisite req && thisScreenplay.Visited(req))
                GUI.color = new Color(GUI.color.r * 0.25f, GUI.color.g, GUI.color.b * 0.25f, GUI.color.a);

            if (Value is Event e)
                e.Name = GUILayout.TextField(e.Name, YNode.Editor.Resources.Styles.NodeHeader, GUILayout.Height(30));
            else
                base.OnHeaderGUI();
            GUI.color = textColor;
        }

        public override Color GetTint()
        {
            var baseTint = base.GetTint();
            return InPreviewPath ? baseTint * new Color(1.2f, 1.2f, 1.5f, 1.2f) : baseTint;
        }
    }
}
