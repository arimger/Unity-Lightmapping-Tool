using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Toolbox.Lighting.Editor
{
    internal static class EditorHelper
    {
        internal static  ReorderableList CreateList(SerializedProperty property, GUIContent label = null)
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
    }
}
