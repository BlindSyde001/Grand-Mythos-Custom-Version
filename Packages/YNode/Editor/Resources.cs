using UnityEditor;
using UnityEngine;

namespace YNode.Editor
{
    public static class Resources
    {
        private static Texture2D? s_dot;
        private static Texture2D? s_dotOuter;
        private static Texture2D? s_nodeBody;
        private static Texture2D? s_nodeHighlight;
        private static Texture2D? s_tooltip;

        private static StyleRepository? _styles;

        // Textures
        public static Texture2D Dot => s_dot != null ? s_dot : s_dot = UnityEngine.Resources.Load<Texture2D>("xnode_dot");

        public static Texture2D DotOuter =>
            s_dotOuter != null ? s_dotOuter : s_dotOuter = UnityEngine.Resources.Load<Texture2D>("xnode_dot_outer");

        public static Texture2D NodeBody =>
            s_nodeBody != null ? s_nodeBody : s_nodeBody = UnityEngine.Resources.Load<Texture2D>("xnode_node");

        public static Texture2D NodeHighlight => s_nodeHighlight != null
            ? s_nodeHighlight
            : s_nodeHighlight = UnityEngine.Resources.Load<Texture2D>("xnode_node_highlight");

        public static Texture2D Tooltip
        {
            get
            {
                if (s_tooltip != null)
                    return s_tooltip;

                s_tooltip = new Texture2D(1, 1);
                s_tooltip.SetPixel(0, 0, new Color(0.25f, 0.25f, 0.25f, 1));
                s_tooltip.Apply();
                return s_tooltip;
            }
        }

        // Styles
        public static StyleRepository Styles => _styles ??= new StyleRepository();
        public static GUIStyle OutputPort => new(EditorStyles.label) { alignment = TextAnchor.MiddleRight };

        public static Texture2D GenerateGridTexture(Color line, Color bg)
        {
            Texture2D tex = new Texture2D(64, 64);
            Color[] cols = new Color[64 * 64];
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    Color col = bg;
                    if (y % 16 == 0 || x % 16 == 0) col = Color.Lerp(line, bg, 0.65f);
                    if (y == 63 || x == 63) col = Color.Lerp(line, bg, 0.35f);
                    cols[(y * 64) + x] = col;
                }
            }

            tex.SetPixels(cols);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            tex.name = "Grid";
            tex.Apply();
            return tex;
        }

        public static Texture2D GenerateCrossTexture(Color line)
        {
            Texture2D tex = new Texture2D(64, 64);
            Color[] cols = new Color[64 * 64];
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    Color col = line;
                    if (y != 31 && x != 31) col.a = 0;
                    cols[(y * 64) + x] = col;
                }
            }

            tex.SetPixels(cols);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.name = "Grid";
            tex.Apply();
            return tex;
        }

        public class StyleRepository
        {
            public GUIStyle InputPort, OutputPort, NodeHeader, NodeBody, Tooltip, NodeHighlight;

            public StyleRepository()
            {
                GUIStyle baseStyle = new GUIStyle("Label");
                baseStyle.fixedHeight = 18;

                InputPort = new GUIStyle(baseStyle);
                InputPort.alignment = TextAnchor.UpperLeft;
                InputPort.padding.left = 0;
                InputPort.active.background = Dot;
                InputPort.normal.background = DotOuter;

                OutputPort = new GUIStyle(baseStyle);
                OutputPort.alignment = TextAnchor.UpperRight;
                OutputPort.padding.right = 0;
                OutputPort.active.background = Dot;
                OutputPort.normal.background = DotOuter;

                NodeHeader = new GUIStyle();
                NodeHeader.alignment = TextAnchor.MiddleCenter;
                NodeHeader.fontStyle = FontStyle.Bold;
                NodeHeader.normal.textColor = Color.white;

                NodeBody = new GUIStyle();
                NodeBody.normal.background = Resources.NodeBody;
                NodeBody.border = new RectOffset(32, 32, 32, 32);
                NodeBody.padding = new RectOffset(16, 16, 4, 16);

                NodeHighlight = new GUIStyle();
                NodeHighlight.normal.background = Resources.NodeHighlight;
                NodeHighlight.border = new RectOffset(32, 32, 32, 32);

                Tooltip = new GUIStyle("helpBox");
                Tooltip.alignment = TextAnchor.MiddleCenter;
                Tooltip.normal.background = Resources.Tooltip;
                Tooltip.active.background = Resources.Tooltip;
                Tooltip.focused.background = Resources.Tooltip;
                Tooltip.hover.background = Resources.Tooltip;
            }
        }
    }
}
