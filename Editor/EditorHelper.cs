using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Toolbox.Lighting.Editor
{
    internal static class EditorHelper
    {
        private static void PreventContextAction()
        {
            switch (Event.current.type)
            {
                case EventType.ContextClick:
                    Event.current.Use();
                    break;
            }
        }


        internal static ReorderableList CreateList(SerializedProperty property, GUIContent label = null)
        {
            return new ReorderableList(property.serializedObject, property, true, true, true, true)
            {
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var element = property.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(rect, element, element.isExpanded);
                },
                elementHeightCallback = (index) =>
                {
                    var element = property.GetArrayElementAtIndex(index);
                    return EditorGUI.GetPropertyHeight(element);
                },
                drawHeaderCallback = (rect) =>
                {
                    var newLabel = EditorGUI.BeginProperty(rect, label, property);
                    EditorGUI.LabelField(rect, newLabel);
                    EditorGUI.EndProperty();
                },
            };
        }

        internal static void DrawNativeList(ref Rect position, SerializedProperty property, bool preventActions, bool isFixed,
            Action<Rect, SerializedProperty, int, GUIContent> drawElementAction, string elementLabel = null)
        {
            var label = EditorGUI.BeginProperty(position, new GUIContent(property.displayName), property);
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (property.isExpanded)
            {
                var targetProperty = property.Copy();
                var endingProperty = property.GetEndProperty();
                var index = 0;

                EditorGUI.indentLevel++;
                targetProperty.NextVisible(true);
                using (new EditorGUI.DisabledScope(isFixed))
                {
                    EditorGUI.PropertyField(position, targetProperty.Copy());
                }

                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                while (targetProperty.NextVisible(false))
                {
                    if (SerializedProperty.EqualContents(targetProperty, endingProperty))
                    {
                        break;
                    }

                    if (preventActions)
                    {
                        PreventContextAction();
                    }

                    var element = targetProperty.Copy();
                    var content = elementLabel != null
                        ? new GUIContent($"{elementLabel} {index}")
                        : new GUIContent(element.displayName);
                    drawElementAction(position, element, index, content);
                    position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    index++;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        internal static void DrawNativeList(SerializedProperty property, bool preventActions, bool isFixed,
            Action<SerializedProperty, int, GUIContent> drawElementAction, string elementLabel = null)
        {
            using (var group = new EditorGUILayout.VerticalScope())
            {
                var label = EditorGUI.BeginProperty(group.rect, new GUIContent(property.displayName), property);
                property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, true);
                if (property.isExpanded)
                {
                    var targetProperty = property.Copy();
                    var endingProperty = property.GetEndProperty();
                    var index = 0;

                    EditorGUI.indentLevel++;
                    targetProperty.NextVisible(true);
                    using (new EditorGUI.DisabledScope(isFixed))
                    {
                        EditorGUILayout.PropertyField(targetProperty.Copy());
                    }

                    while (targetProperty.NextVisible(false))
                    {
                        if (SerializedProperty.EqualContents(targetProperty, endingProperty))
                        {
                            break;
                        }

                        if (preventActions)
                        {
                            PreventContextAction();
                        }

                        var element = targetProperty.Copy();
                        var content = elementLabel != null
                            ? new GUIContent($"{elementLabel} {index}")
                            : new GUIContent(element.displayName);
                        drawElementAction(element, index, content);
                        index++;
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUI.EndProperty();
            }
        }
    }
}
