using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace YNode.Editor
{
    public enum NoodlePath
    {
        Curvy,
        Straight,
        Angled,
        ShaderLab
    }

    public static class Preferences
    {
        /// <summary> The last editor we checked. This should be the one we modify </summary>
        private static GraphWindow? s_lastEditor;

        /// <summary> The last key we checked. This should be the one we modify </summary>
        private static string s_lastKey = "xNode.Settings";

        private static Dictionary<Type, Color> s_typeColors = new Dictionary<Type, Color>();
        private static Dictionary<string, Settings> s_settings = new Dictionary<string, Settings>();

        [Serializable]
        public class Settings : ISerializationCallbackReceiver
        {
            [SerializeField] private Color32 _gridLineColor = new Color(.23f, .23f, .23f);

            [SerializeField] private Color32 _gridBgColor = new Color(.19f, .19f, .19f);

            public float MaxZoom = 5f;
            public float MinZoom = 1f;
            public Color32 TintColor = new Color32(90, 97, 105, 255);
            public Color32 HighlightColor = new Color32(255, 255, 255, 255);
            public bool GridSnap = true;
            public bool AutoSave = true;
            public bool OpenOnCreate = true;
            public bool DragToCreate = true;
            public bool CreateFilter = true;
            public bool ZoomToMouse = true;
            public bool PortTooltips = true;
            [SerializeField] private string TypeColorsData = "";
            public NoodlePath NoodlePath = NoodlePath.Curvy;
            public float NoodleThickness = 2f;

            private Texture2D? _crossTexture;

            private Texture2D? _gridTexture;
            [NonSerialized] public Dictionary<string, Color> TypeColors = new Dictionary<string, Color>();

            public Color32 GridLineColor
            {
                get { return _gridLineColor; }
                set
                {
                    _gridLineColor = value;
                    _gridTexture = null;
                    _crossTexture = null;
                }
            }

            public Color32 GridBgColor
            {
                get { return _gridBgColor; }
                set
                {
                    _gridBgColor = value;
                    _gridTexture = null;
                }
            }

            public Texture2D GridTexture
            {
                get
                {
                    if (_gridTexture == null)
                        _gridTexture = Resources.GenerateGridTexture(GridLineColor, GridBgColor);
                    return _gridTexture;
                }
            }

            public Texture2D CrossTexture
            {
                get
                {
                    if (_crossTexture == null)
                        _crossTexture = Resources.GenerateCrossTexture(GridLineColor);
                    return _crossTexture;
                }
            }

            public void OnAfterDeserialize()
            {
                // Deserialize typeColorsData
                TypeColors = new Dictionary<string, Color>();
                string[] data = TypeColorsData.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < data.Length; i += 2)
                {
                    Color col;
                    if (ColorUtility.TryParseHtmlString("#" + data[i + 1], out col))
                    {
                        TypeColors.Add(data[i], col);
                    }
                }
            }

            public void OnBeforeSerialize()
            {
                // Serialize typeColors
                TypeColorsData = "";
                foreach (var item in TypeColors)
                {
                    TypeColorsData += item.Key + "," + ColorUtility.ToHtmlStringRGB(item.Value) + ",";
                }
            }
        }

        /// <summary> Get settings of current active editor </summary>
        public static Settings GetSettings()
        {
            if (GraphWindow.Current == null) return new Settings();

            if (s_lastEditor != GraphWindow.Current)
            {
                s_lastEditor = GraphWindow.Current;
                s_lastKey = s_lastEditor.PreferenceKey;
            }

            if (!s_settings.ContainsKey(s_lastKey))
                VerifyLoaded();
            return s_settings[s_lastKey];
        }

#if UNITY_2019_1_OR_NEWER
        [SettingsProvider]
        public static SettingsProvider CreateXNodeSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider("Preferences/Node Editor", SettingsScope.User)
            {
                guiHandler = searchContext => { PreferencesGUI(); },
                keywords = new HashSet<string>(new[]
                    { "xNode", "node", "editor", "graph", "connections", "noodles", "ports" })
            };
            return provider;
        }
#endif

#if !UNITY_2019_1_OR_NEWER
        [PreferenceItem("Node Editor")]
#endif
        private static void PreferencesGUI()
        {
            VerifyLoaded();
            Settings settings = s_settings[s_lastKey];

            if (GUILayout.Button(new GUIContent("Documentation", "https://github.com/Siccity/xNode/wiki"),
                    GUILayout.Width(100))) Application.OpenURL("https://github.com/Siccity/xNode/wiki");
            EditorGUILayout.Space();

            NodeSettingsGUI(s_lastKey, settings);
            GridSettingsGUI(s_lastKey, settings);
            SystemSettingsGUI(s_lastKey, settings);
            TypeColorsGUI(s_lastKey, settings);
            if (GUILayout.Button(new GUIContent("Set Default", "Reset all values to default"), GUILayout.Width(120)))
            {
                ResetPrefs();
            }
        }

        private static void GridSettingsGUI(string key, Settings settings)
        {
            //Label
            EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
            settings.GridSnap = EditorGUILayout.Toggle(new GUIContent("Snap", "Hold CTRL in editor to invert"),
                settings.GridSnap);
            settings.ZoomToMouse =
                EditorGUILayout.Toggle(new GUIContent("Zoom to Mouse", "Zooms towards mouse position"),
                    settings.ZoomToMouse);
            EditorGUILayout.LabelField("Zoom");
            EditorGUI.indentLevel++;
            settings.MaxZoom =
                EditorGUILayout.FloatField(new GUIContent("Max", "Upper limit to zoom"), settings.MaxZoom);
            settings.MinZoom =
                EditorGUILayout.FloatField(new GUIContent("Min", "Lower limit to zoom"), settings.MinZoom);
            EditorGUI.indentLevel--;
            settings.GridLineColor = EditorGUILayout.ColorField("Color", settings.GridLineColor);
            settings.GridBgColor = EditorGUILayout.ColorField(" ", settings.GridBgColor);
            if (GUI.changed)
            {
                SavePrefs(key, settings);

                GraphWindow.RepaintAll();
            }

            EditorGUILayout.Space();
        }

        private static void SystemSettingsGUI(string key, Settings settings)
        {
            //Label
            EditorGUILayout.LabelField("System", EditorStyles.boldLabel);
            settings.AutoSave =
                EditorGUILayout.Toggle(new GUIContent("Autosave", "Disable for better editor performance"),
                    settings.AutoSave);
            settings.OpenOnCreate =
                EditorGUILayout.Toggle(
                    new GUIContent("Open Editor on Create",
                        "Disable to prevent openening the editor when creating a new graph"), settings.OpenOnCreate);
            if (GUI.changed) SavePrefs(key, settings);
            EditorGUILayout.Space();
        }

        private static void NodeSettingsGUI(string key, Settings settings)
        {
            //Label
            EditorGUILayout.LabelField("Node", EditorStyles.boldLabel);
            settings.TintColor = EditorGUILayout.ColorField("Tint", settings.TintColor);
            settings.HighlightColor = EditorGUILayout.ColorField("Selection", settings.HighlightColor);
            settings.NoodlePath = (NoodlePath)EditorGUILayout.EnumPopup("Noodle path", settings.NoodlePath);
            settings.NoodleThickness = EditorGUILayout.FloatField(
                new GUIContent("Noodle thickness", "Noodle Thickness of the node connections"),
                settings.NoodleThickness);
            settings.PortTooltips = EditorGUILayout.Toggle("Port Tooltips", settings.PortTooltips);
            settings.DragToCreate =
                EditorGUILayout.Toggle(
                    new GUIContent("Drag to Create",
                        "Drag a port connection anywhere on the grid to create and connect a node"),
                    settings.DragToCreate);
            settings.CreateFilter =
                EditorGUILayout.Toggle(
                    new GUIContent("Create Filter", "Only show nodes that are compatible with the selected port"),
                    settings.CreateFilter);

            //END
            if (GUI.changed)
            {
                SavePrefs(key, settings);
                GraphWindow.RepaintAll();
            }

            EditorGUILayout.Space();
        }

        private static void TypeColorsGUI(string key, Settings settings)
        {
            //Label
            EditorGUILayout.LabelField("Types", EditorStyles.boldLabel);

            //Clone keys so we can enumerate the dictionary and make changes.
            var typeColorKeys = new List<Type>(s_typeColors.Keys);

            //Display type colors. Save them if they are edited by the user
            foreach (var type in typeColorKeys)
            {
                string typeColorKey = type.PrettyName();
                Color col = s_typeColors[type];
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                col = EditorGUILayout.ColorField(typeColorKey, col);
                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    s_typeColors[type] = col;
                    settings.TypeColors[typeColorKey] = col;
                    SavePrefs(key, settings);
                    GraphWindow.RepaintAll();
                }
            }
        }

        /// <summary> Load prefs if they exist. Create if they don't </summary>
        private static Settings LoadPrefs()
        {
            // Create settings if it doesn't exist
            if (!EditorPrefs.HasKey(s_lastKey))
            {
                if (s_lastEditor != null)
                    EditorPrefs.SetString(s_lastKey, JsonUtility.ToJson(s_lastEditor.GetDefaultPreferences()));
                else EditorPrefs.SetString(s_lastKey, JsonUtility.ToJson(new Settings()));
            }

            return JsonUtility.FromJson<Settings>(EditorPrefs.GetString(s_lastKey));
        }

        /// <summary> Delete all prefs </summary>
        public static void ResetPrefs()
        {
            if (EditorPrefs.HasKey(s_lastKey)) EditorPrefs.DeleteKey(s_lastKey);
            if (s_settings.ContainsKey(s_lastKey)) s_settings.Remove(s_lastKey);
            s_typeColors = new Dictionary<Type, Color>();
            VerifyLoaded();
            GraphWindow.RepaintAll();
        }

        /// <summary> Save preferences in EditorPrefs </summary>
        private static void SavePrefs(string key, Settings settings)
        {
            EditorPrefs.SetString(key, JsonUtility.ToJson(settings));
        }

        /// <summary> Check if we have loaded settings for given key. If not, load them </summary>
        private static void VerifyLoaded()
        {
            if (!s_settings.ContainsKey(s_lastKey)) s_settings.Add(s_lastKey, LoadPrefs());
        }

        /// <summary> Return color based on type </summary>
        public static Color GetTypeColor(Type? type)
        {
            VerifyLoaded();
            if (type == null)
                return Color.gray;

            Color col;
            if (!s_typeColors.TryGetValue(type, out col))
            {
                string typeName = type.PrettyName();
                if (s_settings[s_lastKey].TypeColors.ContainsKey(typeName))
                {
                    s_typeColors.Add(type, s_settings[s_lastKey].TypeColors[typeName]);
                }
                else if (type.TryGetAttributeTint(out col))
                {
                    s_typeColors.Add(type, col);
                }
                else
                {
#if UNITY_5_4_OR_NEWER
                    Random.State oldState = Random.state;
                    Random.InitState(typeName.GetHashCode());
#else
                    int oldSeed = UnityEngine.Random.seed;
                    UnityEngine.Random.seed = typeName.GetHashCode();
#endif
                    col = Color.HSVToRGB(Random.Range(0f, 1f), 0.25f, 0.5f);
                    s_typeColors.Add(type, col);
#if UNITY_5_4_OR_NEWER
                    Random.state = oldState;
#else
                    UnityEngine.Random.seed = oldSeed;
#endif
                }
            }

            return col;
        }
    }
}
