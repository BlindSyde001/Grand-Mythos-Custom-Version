using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(ConstrainedTypeAttribute))]
public class TypeConstrainedDrawer : PropertyDrawer
{
    static Dictionary<Type, Type[]> _assignableTypes = new();
    static MethodInfo _method;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // First get the attribute since it contains the range for the slider
        float width = position.height;
        ConstrainedTypeAttribute constraint = (ConstrainedTypeAttribute)attribute;
        var buttonRect = position;
        buttonRect.x += buttonRect.width - width;
        buttonRect.width = width;
        if (GUI.Button(buttonRect, ""))
        {
            ShowObjectSelector(constraint.Type, property, false, onObjectSelectorClosed: o =>
            {
                property.objectReferenceValue = o;
                property.serializedObject.ApplyModifiedProperties();
            });
        }

        EditorGUI.ObjectField(position, property, label);

        if (GUI.Button(buttonRect, "")) { }
    }

    public static void ShowObjectSelector(Type type,
        SerializedProperty prop,
        bool allowSceneObjects,
        List<int> allowedInstanceIDs = null,
        System.Action<Object> onObjectSelectorClosed = null,
        System.Action<Object> onObjectSelectedUpdated = null)
    {
        if (_assignableTypes.TryGetValue(type, out var types) == false)
            _assignableTypes[type] = types = type.Assembly.GetTypes().Where(x => typeof(IAction).IsAssignableFrom(x)).ToArray();

        var objectSelectorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ObjectSelector");
        var objectSelectorGetter = objectSelectorType.GetProperty("get", BindingFlags.Static | BindingFlags.Public);
        var obj = objectSelectorGetter.GetValue(null);
        if (_method == null)
        {
            var parameters = new []{
                typeof(System.Type[]),
                typeof(SerializedProperty),
                typeof(bool),
                typeof(List<int>),
                typeof(System.Action<UnityEngine.Object>),
                typeof(System.Action<UnityEngine.Object>)
            };

            _method = obj.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(x => x.Name == "Show")
                .First(method => method.GetParameters().Select(param => param.ParameterType).SequenceEqual(parameters));
        }

        _method.Invoke(obj, new object[]
        {
            types, // requiredTypes
            prop,
            allowSceneObjects,
            allowedInstanceIDs,
            onObjectSelectorClosed,
            onObjectSelectedUpdated
        });
    }
}