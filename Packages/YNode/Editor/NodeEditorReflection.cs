using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using YNode;
#if UNITY_2019_1_OR_NEWER && USE_ADVANCED_GENERIC_MENU
using GenericMenu = XNodeEditor.AdvancedGenericMenu;
#endif

namespace YNode.Editor
{
    /// <summary> Contains reflection-related extensions built for xNode </summary>
    public static class NodeEditorReflection
    {
        [NonSerialized] private static Dictionary<Type, Color>? s_nodeTint;
        [NonSerialized] private static Dictionary<Type, int>? s_nodeWidth;

        [NonSerialized] private static Type[]? s_nodeTypes = null;

        /// <summary> All available node types </summary>
        public static Type[] NodeTypes => s_nodeTypes ??= GetDerivedTypes(typeof(INodeValue));

        /// <summary> Return a delegate used to determine whether window is docked or not. It is faster to cache this delegate than run the reflection required each time. </summary>
        public static Func<bool> GetIsDockedDelegate(this EditorWindow window)
        {
            BindingFlags fullBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                       BindingFlags.Static;
            MethodInfo isDockedMethod = typeof(EditorWindow).GetProperty("docked", fullBinding)!.GetGetMethod(true);
            return (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), window, isDockedMethod);
        }

        /// <summary> Custom node tint colors defined with [NodeColor(r, g, b)] </summary>
        public static bool TryGetAttributeTint(this Type nodeType, out Color tint)
        {
            if (s_nodeTint == null)
            {
                CacheAttributes<Color, NodeTintAttribute>(out s_nodeTint, x => x.color);
            }

            return s_nodeTint.TryGetValue(nodeType, out tint);
        }

        /// <summary> Get custom node widths defined with [NodeWidth(width)] </summary>
        public static bool TryGetAttributeWidth(this Type nodeType, out int width)
        {
            if (s_nodeWidth == null)
            {
                CacheAttributes<int, NodeWidthAttribute>(out s_nodeWidth, x => x.width);
            }

            return s_nodeWidth.TryGetValue(nodeType, out width);
        }

        private static void CacheAttributes<TV, TA>(out Dictionary<Type, TV> dict, Func<TA, TV> getter) where TA : Attribute
        {
            dict = new();
            for (int i = 0; i < NodeTypes.Length; i++)
            {
                object[] attribs = NodeTypes[i].GetCustomAttributes(typeof(TA), true);
                if (attribs.Length == 0) continue;
                TA attrib = (TA)attribs[0];
                dict.Add(NodeTypes[i], getter(attrib));
            }
        }

        /// <summary> Get all classes deriving from baseType via reflection </summary>
        public static Type[] GetDerivedTypes(this Type baseType)
        {
            List<Type> types = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    types.AddRange(assembly.GetTypes().Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t))
                        .ToArray());
                }
                catch (ReflectionTypeLoadException)
                {
                }
            }

            return types.ToArray();
        }

        /// <summary> Find methods marked with the [ContextMenu] attribute and add them to the context menu </summary>
        public static void AddCustomContextMenuItems(this GenericMenu contextMenu, object obj)
        {
            KeyValuePair<ContextMenu, MethodInfo>[] items = GetContextMenuMethods(obj);
            if (items.Length != 0)
            {
                contextMenu.AddSeparator("");
                List<string> invalidatedEntries = new List<string>();
                foreach (KeyValuePair<ContextMenu, MethodInfo> checkValidate in items)
                {
                    if (checkValidate.Key.validate && !(bool)checkValidate.Value.Invoke(obj, null))
                    {
                        invalidatedEntries.Add(checkValidate.Key.menuItem);
                    }
                }

                for (int i = 0; i < items.Length; i++)
                {
                    KeyValuePair<ContextMenu, MethodInfo> kvp = items[i];
                    if (invalidatedEntries.Contains(kvp.Key.menuItem))
                    {
                        contextMenu.AddDisabledItem(new GUIContent(kvp.Key.menuItem));
                    }
                    else
                    {
                        contextMenu.AddItem(new GUIContent(kvp.Key.menuItem), false, () => kvp.Value.Invoke(obj, null));
                    }
                }
            }
        }

        public static KeyValuePair<ContextMenu, MethodInfo>[] GetContextMenuMethods(object obj)
        {
            Type type = obj.GetType();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                                   BindingFlags.NonPublic);
            List<KeyValuePair<ContextMenu, MethodInfo>> kvp = new List<KeyValuePair<ContextMenu, MethodInfo>>();
            for (int i = 0; i < methods.Length; i++)
            {
                ContextMenu[] attribs = methods[i].GetCustomAttributes(typeof(ContextMenu), true)
                    .Select(x => (ContextMenu)x).ToArray();
                if (attribs.Length == 0) continue;
                if (methods[i].GetParameters().Length != 0)
                {
                    Debug.LogWarning("Method " + methods[i].DeclaringType!.Name + "." + methods[i].Name +
                                     " has parameters and cannot be used for context menu commands.");
                    continue;
                }

                if (methods[i].IsStatic)
                {
                    Debug.LogWarning("Method " + methods[i].DeclaringType!.Name + "." + methods[i].Name +
                                     " is static and cannot be used for context menu commands.");
                    continue;
                }

                for (int k = 0; k < attribs.Length; k++)
                {
                    kvp.Add(new KeyValuePair<ContextMenu, MethodInfo>(attribs[k], methods[i]));
                }
            }
#if UNITY_5_5_OR_NEWER
            //Sort menu items
            kvp.Sort((x, y) => x.Key.priority.CompareTo(y.Key.priority));
#endif
            return kvp.ToArray();
        }

        /// <summary> Very crude. Uses a lot of reflection. </summary>
        public static void OpenPreferences()
        {
            try
            {
#if UNITY_2018_3_OR_NEWER
                SettingsService.OpenUserPreferences("Preferences/Node Editor");
#else
                //Open preferences window
                Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.EditorWindow));
                Type type = assembly.GetType("UnityEditor.PreferencesWindow");
                type.GetMethod("ShowPreferencesWindow", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);

                //Get the window
                EditorWindow window = EditorWindow.GetWindow(type);

                //Make sure custom sections are added (because waiting for it to happen automatically is too slow)
                FieldInfo refreshField =
 type.GetField("m_RefreshCustomPreferences", BindingFlags.NonPublic | BindingFlags.Instance);
                if ((bool) refreshField.GetValue(window)) {
                    type.GetMethod("AddCustomSections", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(window, null);
                    refreshField.SetValue(window, false);
                }

                //Get sections
                FieldInfo sectionsField = type.GetField("m_Sections", BindingFlags.Instance | BindingFlags.NonPublic);
                IList sections = sectionsField.GetValue(window) as IList;

                //Iterate through sections and check contents
                Type sectionType = sectionsField.FieldType.GetGenericArguments() [0];
                FieldInfo sectionContentField =
 sectionType.GetField("content", BindingFlags.Instance | BindingFlags.Public);
                for (int i = 0; i < sections.Count; i++) {
                    GUIContent sectionContent = sectionContentField.GetValue(sections[i]) as GUIContent;
                    if (sectionContent.text == "Node Editor") {
                        //Found contents - Set index
                        FieldInfo sectionIndexField =
 type.GetField("m_SelectedSectionIndex", BindingFlags.Instance | BindingFlags.NonPublic);
                        sectionIndexField.SetValue(window, i);
                        return;
                    }
                }
#endif
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogWarning(
                    "Unity has changed around internally. Can't open properties through reflection. Please contact xNode developer and supply unity version number.");
            }
        }
    }
}
