using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YNode.Editor
{
    /// <summary> A set of editor-only utilities and extensions for xNode </summary>
    public static class Utilities
    {
        /// <summary>C#'s Script Icon [The one MonoBhevaiour Scripts have].</summary>
        private static Texture2D scriptIcon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;

        private static Dictionary<Type, Type> _valueToEditor = new();

        public static Type GetCustomEditor(Type valueType, Type openGeneric, Type genericBase)
        {
            if (_valueToEditor.TryGetValue(valueType, out var editorType))
                return editorType;

            var matchingTypes = TypeCache.GetTypesDerivedFrom(genericBase)
                .Where(x => x.IsAbstract == false)
                .Select(x => (type:x, generics:x.GetArgumentsOfInheritedOpenGenericClass(openGeneric)))
                .Where(x => x.generics != null && x.generics[0].IsAssignableFrom(valueType))
                .Select(x => (x.type, generic: x.generics[0]))
                .ToArray();

            int max = 0;
            Type selection = typeof(NodeEditor);
            foreach (var (type, generic) in matchingTypes)
            {
                var score = 1;
                for (var t = generic.BaseType; t != null; t = t.BaseType) // Most derived wins
                {
                    score++;
                }

                if (max < score)
                {
                    max = score;
                    selection = type;
                }
            }

            _valueToEditor[valueType] = selection;
            return selection;
        }

        public static bool GetAttrib<T>(Type classType, [MaybeNullWhen(false)] out T attribOut) where T : Attribute
        {
            object[] attribs = classType.GetCustomAttributes(typeof(T), false);
            return GetAttrib(attribs, out attribOut);
        }

        public static bool GetAttrib<T>(object[] attribs, [MaybeNullWhen(false)] out T attribOut) where T : Attribute
        {
            for (int i = 0; i < attribs.Length; i++)
            {
                if (attribs[i] is T a)
                {
                    attribOut = a;
                    return true;
                }
            }

            attribOut = null;
            return false;
        }

        public static bool IsMac()
        {
#if UNITY_2017_1_OR_NEWER
            return SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX;
#else
            return SystemInfo.operatingSystem.StartsWith("Mac");
#endif
        }


        /// <summary> Return a prettiefied type name. </summary>
        public static string PrettyName(this Type? type)
        {
            if (type == null) return "null";
            if (type == typeof(System.Object)) return "object";
            if (type == typeof(float)) return "float";
            else if (type == typeof(int)) return "int";
            else if (type == typeof(long)) return "long";
            else if (type == typeof(double)) return "double";
            else if (type == typeof(string)) return "string";
            else if (type == typeof(bool)) return "bool";
            else if (type.IsGenericType)
            {
                string s;
                Type genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(List<>)) s = "List";
                else s = type.GetGenericTypeDefinition().ToString();

                Type[] types = type.GetGenericArguments();
                string[] stypes = new string[types.Length];
                for (int i = 0; i < types.Length; i++)
                {
                    stypes[i] = types[i].PrettyName();
                }

                return s + "<" + string.Join(", ", stypes) + ">";
            }
            else if (type.IsArray)
            {
                string rank = "";
                for (int i = 1; i < type.GetArrayRank(); i++)
                {
                    rank += ",";
                }

                Type elementType = type.GetElementType()!;
                if (!elementType.IsArray) return elementType.PrettyName() + "[" + rank + "]";
                else
                {
                    string s = elementType.PrettyName();
                    int i = s.IndexOf('[');
                    return s.Substring(0, i) + "[" + rank + "]" + s.Substring(i);
                }
            }
            else return type.ToString();
        }

        /// <summary> Returns the default creation path for the node type. </summary>
        public static string NodeDefaultPath(Type type)
        {
            string typePath = type.ToString().Replace('.', '/');
            // Automatically remove redundant 'Node' postfix
            if (typePath.EndsWith("Node"))
                typePath = typePath[..typePath.LastIndexOf("Node", StringComparison.Ordinal)];
            typePath = ObjectNames.NicifyVariableName(typePath);
            return typePath;
        }

        /// <summary>Creates a new C# Class.</summary>
        [MenuItem("Assets/Create/xNode/Node C# Script", false, 89)]
        private static void CreateNode()
        {
            string[] guids = AssetDatabase.FindAssets("xNode_NodeTemplate.cs");
            if (guids.Length == 0)
            {
                Debug.LogWarning("xNode_NodeTemplate.cs.txt not found in asset database");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            CreateFromTemplate(
                "NewNode.cs",
                path
            );
        }

        /// <summary>Creates a new C# Class.</summary>
        [MenuItem("Assets/Create/xNode/NodeGraph C# Script", false, 89)]
        private static void CreateGraph()
        {
            string[] guids = AssetDatabase.FindAssets("xNode_NodeGraphTemplate.cs");
            if (guids.Length == 0)
            {
                Debug.LogWarning("xNode_NodeGraphTemplate.cs.txt not found in asset database");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            CreateFromTemplate(
                "NewNodeGraph.cs",
                path
            );
        }

        public static void CreateFromTemplate(string initialName, string templatePath)
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                ScriptableObject.CreateInstance<DoCreateCodeFile>(),
                initialName,
                scriptIcon,
                templatePath
            );
        }

        /// <summary>Creates Script from Template's path.</summary>
        internal static Object? CreateScript(string pathName, string templatePath)
        {
            string className = Path.GetFileNameWithoutExtension(pathName).Replace(" ", string.Empty);

            UTF8Encoding encoding = new UTF8Encoding(true, false);

            if (File.Exists(templatePath))
            {
                // Read procedures.
                StreamReader reader = new StreamReader(templatePath);
                string templateText = reader.ReadToEnd();
                reader.Close();

                templateText = templateText.Replace("#SCRIPTNAME#", className);
                templateText = templateText.Replace("#NOTRIM#", string.Empty);
                // You can replace as many tags you make on your templates, just repeat Replace function
                // e.g.:
                // templateText = templateText.Replace("#NEWTAG#", "MyText");

                // Write procedures.

                StreamWriter writer = new StreamWriter(Path.GetFullPath(pathName), false, encoding);
                writer.Write(templateText);
                writer.Close();

                AssetDatabase.ImportAsset(pathName);
                return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
            }
            else
            {
                Debug.LogError($"The template file was not found: {templatePath}");
                return null;
            }
        }

        /// Inherits from EndNameAction, must override EndNameAction.Action
        public class DoCreateCodeFile : UnityEditor.ProjectWindowCallback.EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                Object? o = CreateScript(pathName, resourceFile);
                ProjectWindowUtil.ShowCreatedAsset(o);
            }
        }
    }
}
