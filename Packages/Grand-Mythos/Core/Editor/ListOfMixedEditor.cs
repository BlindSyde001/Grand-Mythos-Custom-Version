using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityHackyInternals;
using Object = UnityEngine.Object;

namespace Editor
{
    [AllowGUIEnabledForReadonly, DrawerPriority(100000, 100000, 100000)]
    [CustomPropertyDrawer(typeof(ListOfMixed<>), true)]
    public class ListOfMixedEditor : PropertyDrawer
    {
        const float margin = 6;
        bool _foldout = true;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var refList = property.FindPropertyRelative("_refs");
            float height = EditorGUIUtility.singleLineHeight + margin;
            if (_foldout)
            {
                for (int i = 0; i < refList.arraySize; i++)
                {
                    var refProp = refList.GetArrayElementAtIndex(i);
                    if (refProp.managedReferenceValue != null)
                    {
                        height += EditorGUI.GetPropertyHeight(refProp);
                    }
                    else
                    {
                        height += EditorGUIUtility.singleLineHeight;
                    }
                }

                height += margin * refList.arraySize;
            }

            return height;
        }

        public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
        {
            try
            {
                var interfaceType = fieldInfo.FieldType.GenericTypeArguments[0];
                CacheImplementerOfType(interfaceType);

                bool allowSceneObjects = property.serializedObject.targetObject is not ScriptableObject;
                bool hasUnityObject = false;
                bool hasInlineRef = false;

                Type[] implementingTypes;
                lock (InterfaceToImplementationCache)
                {
                    implementingTypes = InterfaceToImplementationCache[interfaceType];
                }

                foreach (var type in implementingTypes)
                {
                    hasUnityObject |= typeof(UnityEngine.Object).IsAssignableFrom(type);
                    hasInlineRef |= typeof(UnityEngine.Object).IsAssignableFrom(type) == false && type.IsInterface == false;
                }

                EditorGUI.BeginProperty(area, label, property);

                var objList = property.FindPropertyRelative("_objects");
                var refList = property.FindPropertyRelative("_refs");

                var lineHeight = EditorGUIUtility.singleLineHeight;
                EditorGUI.DrawRect(area.Expand(1, 1, 1, 1), SirenixGUIStyles.ListItemDragBg);

                { // FOLDOUT HEADER
                    var labelRect = area.AlignTop(lineHeight + margin).AlignLeft(area.width - lineHeight);
                    var buttonRect = area.AlignTop(lineHeight + margin).AlignRight(lineHeight).Expand(0, 1, 0, -1f);

                    if (Event.current.type == EventType.Repaint)
                        SirenixGUIStyles.ToolbarBackground.Draw(labelRect, false, false, false, false);
                    _foldout = EditorGUI.Foldout(labelRect, _foldout, label);
                    GUI.Label(labelRect, objList.arraySize == 0 ? "Empty" : $"{objList.arraySize} items", SirenixGUIStyles.RightAlignedGreyMiniLabel);

                    if (GUI.Button(buttonRect, GUIContent.none, SirenixGUIStyles.ToolbarButton))
                        ((IList)fieldInfo.GetValue(property.serializedObject.targetObject)).Add(null);
                    SdfIcons.DrawIcon(buttonRect.Padding(5), SdfIconType.Plus);
                }

                if (_foldout == false)
                {
                    return;
                }

                var even = SirenixGUIStyles.ListItemColorEven;
                var odd = SirenixGUIStyles.ListItemColorOdd;

                var line = area.AlignTop(lineHeight).AddPosition(0, lineHeight + margin + margin/2);
                EditorGUI.indentLevel++;
                for (int i = 0; i < objList.arraySize; i++)
                {
                    var refProp = refList.GetArrayElementAtIndex(i);
                    var objProp = objList.GetArrayElementAtIndex(i);
                    bool containsInlineInterface = refProp.managedReferenceValue != null;

                    float propHeight;
                    if (containsInlineInterface)
                    {
                        propHeight = EditorGUI.GetPropertyHeight(refProp);
                    }
                    else
                    {
                        propHeight = EditorGUIUtility.singleLineHeight;
                    }

                    EditorGUI.DrawRect(line.SetHeight(propHeight).Expand(0, margin / 2), i % 2 == 0 ? even : odd);

                    Rect fieldRect = line.AlignLeft(line.width - lineHeight);
                    Rect fieldValueRect = EditorGUI.IndentedRect(fieldRect);
                    Rect removeButton = line.AlignRight(lineHeight);

                    { // PROPERTY
                        EditorGUI.BeginChangeCheck();

                        object? newRef = refProp.managedReferenceValue;
                        Object? newObj = objProp.objectReferenceValue;
                        if (hasInlineRef)
                            newRef = InlineObjectPicker(fieldValueRect.Padding(16, 38, 0, 0), fieldValueRect.AlignRight(40).AlignLeft(20), newRef, interfaceType, allowSceneObjects);

                        var previousEnabled = GUI.enabled;
                        GUI.enabled = previousEnabled && hasUnityObject;
                        newObj = UnityObjectField(fieldValueRect, fieldValueRect.AlignRight(60).AlignLeft(20), newObj, interfaceType, allowSceneObjects);

                        GUI.enabled = previousEnabled && hasInlineRef;
                        if (Event.current.type == EventType.Repaint)
                            EditorIcons.StarPointer.Draw(fieldValueRect.AlignRight(40).AlignLeft(20));
                        GUI.enabled = previousEnabled;

                        if (containsInlineInterface)
                        {
                            EditorGUI.PropertyField(fieldRect.SetHeight(propHeight), refProp, GUIContent.none, true);
                        }

                        EditorGUI.LabelField(fieldValueRect.Padding(5, 0, 0, 0), new GUIContent((newRef ?? newObj)?.ToString() ?? $"None ({interfaceType.Name})"));

                        var iconRect = fieldValueRect.AlignLeft(fieldRect.height * 0.75f).SetHeight(fieldRect.height * 0.75f).AddPosition(0f, fieldRect.height * 0.125f);
                        if (newObj is not null && GUIHelper.GetAssetThumbnail(newObj, newObj.GetType(), true) is {} thumbnail)
                        {
                            GUI.DrawTexture(iconRect, thumbnail);
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                            var baseList = (IList)fieldInfo.GetValue(property.serializedObject.targetObject);
                            if (newObj != objProp.objectReferenceValue)
                            {
                                baseList[i] = newObj;
                                objProp.objectReferenceValue = newObj;
                                refProp.managedReferenceValue = null;
                            }
                            else if (newRef != refProp.managedReferenceValue)
                            {
                                baseList[i] = newRef;
                                refProp.managedReferenceValue = newRef;
                                objProp.objectReferenceValue = null;
                            }
                        }
                    }

                    { // REMOVE BUTTON
                        removeButton.height = propHeight;
                        if (GUI.Button(removeButton, GUIContent.none, SirenixGUIStyles.None))
                        {
                            ((IList)fieldInfo.GetValue(property.serializedObject.targetObject)).RemoveAt(i);
                            refList.DeleteArrayElementAtIndex(i);
                            objList.DeleteArrayElementAtIndex(i);
                        }

                        SdfIcons.DrawIcon(removeButton.Padding(5), SdfIconType.X);
                    }

                    line.y += propHeight + margin;
                }
                EditorGUI.indentLevel--;
            }
            finally
            {
                EditorGUI.EndProperty();
            }
        }

        static object? InlineObjectPicker(Rect fieldRect, Rect buttonRect, object? value, Type type, bool allowSceneObjects)
        {
            bool mouseHover = buttonRect.Contains(Event.current.mousePosition);

            var controlId = GUIUtility.GetControlID(FocusType.Keyboard);

            var hasKeyboardFocus = GUIUtility.keyboardControl == controlId && GUIHelper.CurrentWindow == EditorWindow.focusedWindow;

            var objectPicker = ObjectPicker.GetObjectPicker($"{type.FullName}+{GUIUtility.GetControlID(FocusType.Passive)}", type);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && mouseHover
                || hasKeyboardFocus && Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown)
            {
                objectPicker.ShowObjectPicker(value, allowSceneObjects, fieldRect);
            }

            if (objectPicker.IsReadyToClaim && Event.current.type == EventType.Repaint)
            {
                GUI.changed = true;
                return objectPicker.ClaimObject();
            }

            return value;
        }

        static Object? UnityObjectField(Rect rect, Rect editButtonRect, Object? value, Type objectType, bool allowSceneObjects)
        {
            Object? originalValue = value;
            bool originalValueWasFakeNull = value == null && !ReferenceEquals(value, null);

            // This could be added to also support dragging on object fields.
            // value = DragAndDropUtilities.DragAndDropZone(rect, value, objectType, true, true) as UnityEngine.Object;

            var penRect = editButtonRect;
            bool showEditor = value != null;
            if (showEditor)
                BeginDrawOpenInspector(penRect, value, IndentLabelRect(rect, false));

            value = ObjectField(rect, value, objectType, allowSceneObjects);

            if (showEditor)
                EndDrawOpenInspector(penRect, value);

            if (originalValueWasFakeNull && ReferenceEquals(value, null))
            {
                value = originalValue;
            }

            return value;
        }

        static void BeginDrawOpenInspector(Rect rect, Object? obj, Rect btnRect)
        {
            // Setting GUI.enabled to false here can accidentally disable prefix labels drawn just before this is invoked
            // because prefix labels inherit the enabledness of whatever thing is drawn next. So if a prefix label is drawn
            // before a Unity object field for a null value, the prefix label will be disabled because the IconButton below
            // becomes disabled.

            //var prevEnabled = GUI.enabled;
            //GUI.enabled = obj != null;
            if (/*GUI.enabled && */Event.current.isMouse && rect.Contains(Event.current.mousePosition))
            {
                GUIHelper.RequestRepaint();
            }
            if (SirenixEditorGUI.IconButton(rect, EditorIcons.Transparent, "Inspect object") && obj != null)
            {
                if (obj is Sprite && AssetDatabase.Contains(obj))
                {
                    var path = AssetDatabase.GetAssetPath(obj);
                    obj = AssetDatabase.LoadMainAssetAtPath(path) ?? obj;
                }

                if (Event.current.button == 0 || obj is GameObject)
                {
                    GUIHelper.OpenInspectorWindow(obj);
                }
                else if (Event.current.button == 1)
                {
                    OpenEditorInOdinDropDown(obj, btnRect);
                }

                GUIHelper.ExitGUI(true);
            }
            //GUI.enabled = prevEnabled;
        }

        static void EndDrawOpenInspector(Rect rect, Object? obj)
        {
            var prevEnabled = GUI.enabled;
            GUI.enabled = obj != null;
            rect.x -= 2;
            rect = rect.AlignRight(rect.height);
            EditorIcons.Pen.Draw(rect);
            GUI.enabled = prevEnabled;
        }

        static Rect IndentLabelRect(Rect totalPosition, bool hasLabel)
        {
            if (!hasLabel)
            {
                return EditorGUI.IndentedRect(totalPosition);
            }
            else
            {
                return new Rect(totalPosition.x + GUIHelper.BetterLabelWidth, totalPosition.y, totalPosition.width - GUIHelper.BetterLabelWidth, totalPosition.height);
            }
        }

        static void OpenEditorInOdinDropDown(Object obj, Rect btnRect)
        {
            var odinEditorWindow = AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Editor.OdinEditorWindow");
            odinEditorWindow.GetMethods(Flags.StaticPublic)
                .First(x => x.Name == "InspectObjectInDropDown" && x.GetParameters().Last().ParameterType == typeof(float))
                .Invoke(null, new object[] { obj, btnRect, 400 });
        }

        static Object? ObjectField(
            Rect position,
            Object? obj,
            Type objType,
            bool allowSceneObjects)
        {
            int controlId = GUIUtility.GetControlID(s_ObjectFieldHash, FocusType.Keyboard, position);
            Rect dropRect = EditorGUI.IndentedRect(position);
            GUIStyle style = EditorStyles.objectField;
            Type[] additionalTypes;
            lock (InterfaceToImplementationCache)
            {
                additionalTypes = InterfaceToImplementationCache[objType];
            }

            return Internals.CustomDoObjectField(EditorGUI.IndentedRect(position), dropRect, controlId, obj, null, objType, additionalTypes, null, null, allowSceneObjects, style);
        }

        static readonly int s_ObjectFieldHash = nameof (s_ObjectFieldHash).GetHashCode();
        static readonly Dictionary<Type, Type[]> InterfaceToImplementationCache = new();

        static void CacheImplementerOfType(Type type)
        {
            lock (InterfaceToImplementationCache)
            {
                if (InterfaceToImplementationCache.ContainsKey(type))
                    return;

                InterfaceToImplementationCache.Add(type, Array.Empty<Type>());
                Task.Run(() =>
                {
                    try
                    {
                        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(type.IsAssignableFrom).ToArray();
                        lock (InterfaceToImplementationCache)
                            InterfaceToImplementationCache[type] = types;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                });
            }
        }
    }
}