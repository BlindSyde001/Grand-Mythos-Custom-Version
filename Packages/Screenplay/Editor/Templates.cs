using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Screenplay.Editor
{
    public static class Templates
    {
        /// <summary>C#'s Script Icon [The one MonoBehaviour Scripts have].</summary>
        private static Texture2D s_scriptIcon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;

        /// <summary>Creates a new C# Class.</summary>
        [MenuItem("Assets/Create/Screenplay/Interaction Template", false, 89)]
        private static void CreateNode()
        {
            var thisFilePath = GetCallerPath();
            var thisDirectory = Path.GetDirectoryName(thisFilePath);
            var template = Path.Combine(thisDirectory, "TemplateInteraction.cs");
            if (File.Exists(template))
                CreateFromTemplate("MyInteraction.cs", template);
            else
                Debug.LogError($"Could not find template at path {template}");
        }

        static string GetCallerPath([CallerFilePath] string filePath = "")
        {
            return filePath;
        }

        public static void CreateFromTemplate(string initialName, string templatePath)
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                ScriptableObject.CreateInstance<DoCreateCodeFile>(),
                initialName,
                s_scriptIcon,
                templatePath
            );
        }

        /// Inherits from EndNameAction, must override EndNameAction.Action
        public class DoCreateCodeFile : UnityEditor.ProjectWindowCallback.EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                Object? o = CreateScript(pathName, resourceFile);
                if (o != null)
                    ProjectWindowUtil.ShowCreatedAsset(o);
            }
        }

        /// <summary>Creates Script from Template's path.</summary>
        private static Object? CreateScript(string pathName, string templatePath)
        {
            string className = Path.GetFileNameWithoutExtension(pathName).Replace(" ", string.Empty);

            var encoding = new UTF8Encoding(true, false);

            if (File.Exists(templatePath))
            {
                var reader = new StreamReader(templatePath);
                string templateText = reader.ReadToEnd();
                reader.Close();

                templateText = templateText.Replace("__SCRIPTNAME__", className);
                templateText = templateText.Replace("#if _", string.Empty);
                templateText = templateText.Replace("#endif // _", string.Empty);

                var writer = new StreamWriter(Path.GetFullPath(pathName), false, encoding);
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
    }
}
