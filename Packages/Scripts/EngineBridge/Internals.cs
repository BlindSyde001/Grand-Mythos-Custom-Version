using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityHackyInternals
{
    public static class Internals
    {
#if UNITY_EDITOR
        public delegate UnityEngine.Object ObjectFieldValidator(
            UnityEngine.Object[] references,
            System.Type objType,
            SerializedProperty property,
            ObjectFieldValidatorOptions options);

        [Flags]
        public enum ObjectFieldValidatorOptions
        {
            None = 0,
            ExactObjectTypeValidation = 1,
        }

        public static GUIStyle objectFieldButton => EditorStyles.objectFieldButton;

        public static Object CustomDoObjectField(Rect position, Rect dropRect, int id, Object obj, Object objBeingEdited, Type objType, Type[] additionalTypes, SerializedProperty property, ObjectFieldValidator validator, bool allowSceneObjects, GUIStyle style, Action<Object> onObjectSelectorClosed = null, Action<Object> onObjectSelectedUpdated = null)
        {
            EditorGUI.ObjectFieldValidator internalValidator = validator == null ? null : (refs, type, property, options) => validator(refs, type, property, (ObjectFieldValidatorOptions)options);
            return CustomDoObjectField(position, dropRect, id, obj, objBeingEdited, objType, additionalTypes, property, internalValidator, allowSceneObjects, style, EditorStyles.objectFieldButton, onObjectSelectorClosed, onObjectSelectedUpdated);
        }

        static Object CustomDoObjectField(Rect position, Rect dropRect, int id, Object obj, Object objBeingEdited, Type objType, Type[] additionalTypes, SerializedProperty property, EditorGUI.ObjectFieldValidator validator, bool allowSceneObjects, GUIStyle style, GUIStyle buttonStyle, Action<Object> onObjectSelectorClosed = null, Action<Object> onObjectSelectedUpdated = null)
        {
            if (validator == null)
                validator = EditorGUI.ValidateObjectFieldAssignment;
            if (property != null)
                obj = property.objectReferenceValue;
            Event current = Event.current;
            EventType eventType = current.type;
            if (!GUI.enabled && GUIClip.enabled && Event.current.rawType == EventType.MouseDown)
                eventType = Event.current.rawType;
            bool flag = EditorGUIUtility.HasObjectThumbnail(objType);
            EditorGUI.ObjectFieldVisualType visualType = EditorGUI.ObjectFieldVisualType.IconAndText;
            if (flag && position.height <= 18.0 && position.width <= 32.0)
                visualType = EditorGUI.ObjectFieldVisualType.MiniPreview;
            else if (flag && position.height > 18.0)
                visualType = EditorGUI.ObjectFieldVisualType.LargePreview;
            Vector2 iconSize = EditorGUIUtility.GetIconSize();
            switch (visualType)
            {
                case EditorGUI.ObjectFieldVisualType.IconAndText:
                    EditorGUIUtility.SetIconSize(new Vector2(12f, 12f));
                    break;
                case EditorGUI.ObjectFieldVisualType.LargePreview:
                    EditorGUIUtility.SetIconSize(new Vector2(64f, 64f));
                    break;
            }

            if ((eventType == EventType.MouseDown && Event.current.button == 1 || eventType == EventType.ContextClick && visualType == EditorGUI.ObjectFieldVisualType.IconAndText) && position.Contains(Event.current.mousePosition))
            {
                Object actualObject = property != null ? property.objectReferenceValue : obj;
                GenericMenu menu = new GenericMenu();
                if (EditorGUI.FillPropertyContextMenu(property, menu: menu) != null)
                    menu.AddSeparator("");
                menu.AddItem(GUIContent.Temp("Properties..."), false, () => PropertyEditor.OpenPropertyEditor(actualObject));
                menu.DropDown(position);
                Event.current.Use();
            }

            switch (eventType)
            {
                case EventType.MouseDown:
                    if (position.Contains(Event.current.mousePosition) && Event.current.button == 0)
                    {
                        Rect buttonRect = GetButtonRect(visualType, position);
                        EditorGUIUtility.editingTextField = false;
                        if (buttonRect.Contains(Event.current.mousePosition))
                        {
                            if (GUI.enabled)
                            {
                                GUIUtility.keyboardControl = id;
                                Type[] typeArray;
                                if (additionalTypes != null)
                                    typeArray = additionalTypes;
                                else
                                    typeArray = new Type[1] { objType };
                                Type[] requiredTypes = typeArray;
                                if (property != null)
                                    ObjectSelector.get.Show(requiredTypes, property, allowSceneObjects, onObjectSelectorClosed: onObjectSelectorClosed, onObjectSelectedUpdated: onObjectSelectedUpdated);
                                else
                                    ObjectSelector.get.Show(obj, requiredTypes, objBeingEdited, allowSceneObjects, onObjectSelectorClosed: onObjectSelectorClosed, onObjectSelectedUpdated: onObjectSelectedUpdated);
                                ObjectSelector.get.objectSelectorID = id;
                                current.Use();
                                GUIUtility.ExitGUI();
                            }
                        }
                        else
                        {
                            Object @object = property != null ? property.objectReferenceValue : obj;
                            Component component = @object as Component;
                            if ((bool)(Object)component)
                                @object = component.gameObject;
                            if (EditorGUI.showMixedValue)
                                @object = null;
                            if (Event.current.clickCount == 1)
                            {
                                GUIUtility.keyboardControl = id;
                                EditorGUI.PingObjectOrShowPreviewOnClick(@object, position);
                                Material targetMaterial = @object as Material;
                                if (targetMaterial != null)
                                    EditorGUI.PingObjectInSceneViewOnClick(targetMaterial);
                                current.Use();
                            }
                            else if (Event.current.clickCount == 2 && (bool)@object)
                            {
                                AssetDatabase.OpenAsset(@object);
                                current.Use();
                                GUIUtility.ExitGUI();
                            }
                        }
                    }

                    break;
                case EventType.KeyDown:
                    if (GUIUtility.keyboardControl == id)
                    {
                        if (current.keyCode == KeyCode.Backspace || current.keyCode == KeyCode.Delete && (current.modifiers & EventModifiers.Shift) == EventModifiers.None)
                        {
                            if (property != null)
                            {
                                if (property.propertyPath.EndsWith("]"))
                                {
                                    string propertyPath = property.propertyPath.Substring(0, property.propertyPath.LastIndexOf(".Array.data[", StringComparison.Ordinal));
                                    SerializedProperty property1 = property.serializedObject.FindProperty(propertyPath);
                                    if (!PropertyHandler.s_reorderableLists.ContainsKey(ReorderableListWrapper.GetPropertyIdentifier(property1)) && GUI.isInsideList && EditorGUI.GetInsideListDepth() == property1.depth)
                                        TargetChoiceHandler.DeleteArrayElement(property);
                                    else
                                        property.objectReferenceValue = null;
                                }
                                else
                                    property.objectReferenceValue = null;
                            }
                            else
                                obj = null;

                            GUI.changed = true;
                            current.Use();
                        }

                        if (current.MainActionKeyForControl(id))
                        {
                            Type[] typeArray;
                            if (additionalTypes != null)
                                typeArray = additionalTypes;
                            else
                                typeArray = new Type[1] { objType };
                            Type[] requiredTypes = typeArray;
                            if (property != null)
                                ObjectSelector.get.Show(requiredTypes, property, allowSceneObjects);
                            else
                                ObjectSelector.get.Show(obj, requiredTypes, objBeingEdited, allowSceneObjects);
                            ObjectSelector.get.objectSelectorID = id;
                            current.Use();
                            GUIUtility.ExitGUI();
                        }
                    }

                    break;
                case EventType.Repaint:
                    GUIContent content = GUIContent.none;//!EditorGUI.showMixedValue ? obj == null ? GUIContent.none : EditorGUIUtility.ObjectContent(obj, objType, property, validator) : EditorGUI.mixedValueContent;
                    switch (visualType)
                    {
                        case EditorGUI.ObjectFieldVisualType.IconAndText:
                            EditorGUI.BeginHandleMixedValueContentColor();
                            style.Draw(position, content, id, DragAndDrop.activeControlID == id, position.Contains(Event.current.mousePosition));
                            Rect position1 = buttonStyle.margin.Remove(GetButtonRect(visualType, position));
                            buttonStyle.Draw(position1, GUIContent.none, id, DragAndDrop.activeControlID == id, position1.Contains(Event.current.mousePosition));
                            EditorGUI.EndHandleMixedValueContentColor();
                            break;
                        case EditorGUI.ObjectFieldVisualType.LargePreview:
                            DrawObjectFieldLargeThumb(position, id, obj, content);
                            break;
                        case EditorGUI.ObjectFieldVisualType.MiniPreview:
                            DrawObjectFieldMiniThumb(position, id, obj, content);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    string errorString;
                    if (eventType == EventType.DragPerform && !ValidDroppedObject(DragAndDrop.objectReferences, objType, out errorString))
                    {
                        Object objectReference = DragAndDrop.objectReferences[0];
                        EditorUtility.DisplayDialog("Can't assign script", errorString, "OK");
                        break;
                    }

                    if (dropRect.Contains(Event.current.mousePosition) && GUI.enabled)
                    {
                        Object[] objectReferences = DragAndDrop.objectReferences;
                        Object target = validator(objectReferences, objType, property, EditorGUI.ObjectFieldValidatorOptions.None);
                        if (target != null && !allowSceneObjects && !EditorUtility.IsPersistent(target))
                            target = null;
                        if (target != null)
                        {
                            if (DragAndDrop.visualMode == DragAndDropVisualMode.None)
                                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                            if (eventType == EventType.DragPerform)
                            {
                                if (property != null)
                                    property.objectReferenceValue = target;
                                else
                                    obj = target;
                                GUI.changed = true;
                                DragAndDrop.AcceptDrag();
                                DragAndDrop.activeControlID = 0;
                            }
                            else
                                DragAndDrop.activeControlID = id;

                            Event.current.Use();
                        }
                    }

                    break;
                case EventType.ValidateCommand:
                    if ((current.commandName == "Delete" || current.commandName == "SoftDelete") && GUIUtility.keyboardControl == id)
                    {
                        current.Use();
                    }

                    break;
                case EventType.ExecuteCommand:
                    string commandName = current.commandName;
                    if (commandName == "ObjectSelectorUpdated" && ObjectSelector.get.objectSelectorID == id && GUIUtility.keyboardControl == id && (property == null || !property.isScript))
                        return AssignSelectedObject(property, validator, objType, current);
                    if (commandName == "ObjectSelectorClosed" && ObjectSelector.get.objectSelectorID == id && GUIUtility.keyboardControl == id && property != null && property.isScript)
                    {
                        if (ObjectSelector.get.GetInstanceID() != 0)
                            return AssignSelectedObject(property, validator, objType, current);
                        current.Use();
                        break;
                    }

                    if ((current.commandName == "Delete" || current.commandName == "SoftDelete") && GUIUtility.keyboardControl == id)
                    {
                        if (property != null)
                            property.objectReferenceValue = null;
                        else
                            obj = null;
                        GUI.changed = true;
                        current.Use();
                    }

                    break;
                case EventType.DragExited:
                    if (GUI.enabled)
                    {
                        HandleUtility.Repaint();
                    }

                    break;
            }

            EditorGUIUtility.SetIconSize(iconSize);
            return obj;
        }

        private static Rect GetButtonRect(EditorGUI.ObjectFieldVisualType visualType, Rect position)
        {
            switch (visualType)
            {
                case EditorGUI.ObjectFieldVisualType.IconAndText:
                    return new Rect(position.xMax - 19f, position.y, 19f, position.height);
                case EditorGUI.ObjectFieldVisualType.LargePreview:
                    return new Rect(position.xMax - 36f, position.yMax - 14f, 36f, 14f);
                case EditorGUI.ObjectFieldVisualType.MiniPreview:
                    return new Rect(position.xMax - 14f, position.y, 14f, position.height);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Object AssignSelectedObject(SerializedProperty property, EditorGUI.ObjectFieldValidator validator, Type objectType, Event evt)
        {
            Object[] references = new Object[1] { ObjectSelector.GetCurrentObject() };
            Object @object = validator(references, objectType, property, EditorGUI.ObjectFieldValidatorOptions.None);
            if (property != null)
                property.objectReferenceValue = @object;
            GUI.changed = true;
            evt.Use();
            return @object;
        }

        private static void DrawObjectFieldLargeThumb(Rect position, int id, Object obj, GUIContent content)
        {
            GUIStyle objectFieldThumb = EditorStyles.objectFieldThumb;
            objectFieldThumb.Draw(position, GUIContent.none, id, DragAndDrop.activeControlID == id, position.Contains(Event.current.mousePosition));
            if (obj != null && !EditorGUI.showMixedValue)
            {
                Matrix4x4 matrix = GUI.matrix;
                bool flag1 = obj is Sprite;
                bool flag2 = obj is Texture2D && (obj as Texture2D).alphaIsTransparency;
                Rect position1 = objectFieldThumb.padding.Remove(position);
                Texture2D assetPreview = AssetPreview.GetAssetPreview(obj);
                if (assetPreview != null)
                {
                    if (((flag1 ? 1 : (assetPreview.alphaIsTransparency ? 1 : 0)) | (flag2 ? 1 : 0)) != 0)
                        GUI.DrawTexture(position1, EditorGUI.transparentCheckerTexture, ScaleMode.StretchToFill, false);
                    Vector2 vector2 = Vector2.one * 64f;
                    GUIUtility.ScaleAroundPivot(position1.size / vector2, position1.position);
                    position1.size = vector2;
                    GUIStyle.none.Draw(position1, assetPreview, false, false, false, false);
                    GUI.matrix = matrix;
                }
                else
                {
                    if (flag1 | flag2)
                    {
                        GUI.DrawTexture(position1, EditorGUI.transparentCheckerTexture, ScaleMode.StretchToFill, false);
                        GUI.DrawTexture(position1, content.image, ScaleMode.StretchToFill, true);
                    }
                    else
                        EditorGUI.DrawPreviewTexture(position1, content.image);

                    HandleUtility.Repaint();
                }
            }
            else
            {
                GUIStyle guiStyle = objectFieldThumb.name + "Overlay";
                EditorGUI.BeginHandleMixedValueContentColor();
                guiStyle.Draw(position, content, id);
                EditorGUI.EndHandleMixedValueContentColor();
            }

            ((GUIStyle)(objectFieldThumb.name + "Overlay2")).Draw(position, s_Select, id);
        }

        private static GUIContent s_Select = EditorGUIUtility.TrTextContent("Select");

        private static void DrawObjectFieldMiniThumb(Rect position, int id, Object obj, GUIContent content)
        {
            GUIStyle objectFieldMiniThumb = EditorStyles.objectFieldMiniThumb;
            position.width = 32f;
            EditorGUI.BeginHandleMixedValueContentColor();
            bool isHover = obj != null;
            bool on = DragAndDrop.activeControlID == id;
            bool hasKeyboardFocus = GUIUtility.keyboardControl == id;
            objectFieldMiniThumb.Draw(position, isHover, false, on, hasKeyboardFocus);
            EditorGUI.EndHandleMixedValueContentColor();
            if (!(obj != null) || EditorGUI.showMixedValue)
                return;
            Rect position1 = new Rect(position.x + 1f, position.y + 1f, position.height - 2f, position.height - 2f);
            Texture2D image = content.image as Texture2D;
            if (image != null && image.alphaIsTransparency)
                EditorGUI.DrawTextureTransparent(position1, image);
            else
                EditorGUI.DrawPreviewTexture(position1, content.image);
            if (position1.Contains(Event.current.mousePosition))
                GUI.Label(position1, GUIContent.Temp(string.Empty, "Ctrl + Click to show preview"));
        }

        private static bool ValidDroppedObject(Object[] references, Type objType, out string errorString)
        {
            errorString = "";
            if (references == null || references.Length == 0)
                return true;
            Object reference = references[0];
            Object @object = EditorUtility.InstanceIDToObject(reference.GetInstanceID());
            if (!(@object is MonoBehaviour) && !(@object is ScriptableObject) || HasValidScript(@object))
                return true;
            errorString = string.Format("Type cannot be found: {0}. Containing file and class name must match.", reference.GetType());
            return false;
        }

        private static bool HasValidScript(Object obj)
        {
            MonoScript monoScript = MonoScript.FromScriptedObject(obj);
            return !(monoScript == null) && !(monoScript.GetClass() == null);
        }
#endif
    }
}