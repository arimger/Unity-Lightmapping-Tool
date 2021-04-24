using System;
using UnityEditor;
using UnityEngine;

namespace Toolbox.Lighting.Editor
{
    [CustomPropertyDrawer(typeof(LightmapTransitionPreset))]
    internal class LightmapTransitionPresetDrawer : PropertyDrawer
    {
        /// <summary>
        /// Prevents context click events.
        /// </summary>
        private void PreventActions()
        {
            switch (Event.current.type)
            {
                case EventType.ContextClick:
                    Event.current.Use();
                    break;
            }
        }

        /// <summary>
        /// Draws generic list in the native way.
        /// </summary>
        private void DrawNativeList(ref Rect position, SerializedProperty property, bool preventActions, 
            Action<Rect, SerializedProperty, int, GUIContent> drawElementAction, string elementLabel = null)
        {
            var label = EditorGUI.BeginProperty(position, new GUIContent(property.displayName), property);
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (property.isExpanded)
            {
                //cache all needed property references
                var targetProperty = property.Copy();
                var endingProperty = property.GetEndProperty();
                var index = -1;

                EditorGUI.indentLevel++;
                //iterate over all children (but only 1 level depth)
                while (targetProperty.NextVisible(index == -1))
                {
                    if (SerializedProperty.EqualContents(targetProperty, endingProperty))
                    {
                        break;
                    }

                    if (preventActions)
                    {
                        PreventActions();
                    }

                    if (index == -1)
                    {
                        //handle size property
                        using (new EditorGUI.DisabledScope(true))
                        {
                            EditorGUI.PropertyField(position, targetProperty.Copy());
                        }
                    }
                    else
                    {
                        //handle each element
                        var element = targetProperty.Copy();
                        var content = elementLabel != null 
                            ? new GUIContent($"{elementLabel} {index}") 
                            : new GUIContent(element.displayName);
                        drawElementAction(position, element, index, content);
                    }

                    position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    index++;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var blendedPresetsProperty = property.FindPropertyRelative("blendedPresets");
            var allowedIdnexesProperty = property.FindPropertyRelative("allowedIndexes");
            return EditorGUI.GetPropertyHeight(blendedPresetsProperty, label, blendedPresetsProperty.isExpanded) + 
                   EditorGUI.GetPropertyHeight(allowedIdnexesProperty, label, allowedIdnexesProperty.isExpanded);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var blendedPresetsProperty = property.FindPropertyRelative("blendedPresets");
            var allowedIdnexesProperty = property.FindPropertyRelative("allowedIndexes");

            position.yMax = position.yMin + EditorGUIUtility.singleLineHeight;

            EditorGUI.indentLevel++;
            DrawNativeList(ref position, blendedPresetsProperty, true, (rect, element, index, label) =>
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUI.PropertyField(rect, element, label, element.isExpanded);
                }
            }, "Position");
            DrawNativeList(ref position, allowedIdnexesProperty, true, (rect, element, index, label) =>
            {
                EditorGUI.PropertyField(rect, element, label, element.isExpanded);
            }, "Index");
            EditorGUI.indentLevel--;
        }
    }
}