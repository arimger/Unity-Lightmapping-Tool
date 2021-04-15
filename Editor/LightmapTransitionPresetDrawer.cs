using UnityEditor;
using UnityEngine;

namespace Toolbox.Lighting.Editor
{
    [CustomPropertyDrawer(typeof(LightmapTransitionPreset))]
    internal class LightmapTransitionPresetDrawer : PropertyDrawer
    {
        private void PreventActions()
        {
            switch (Event.current.type)
            {
                case EventType.ContextClick:
                    Event.current.Use();
                    break;
            }
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var allowedIdnexesProperty = property.FindPropertyRelative("allowedIndexes");

            EditorGUI.indentLevel++;
            position.yMax = position.yMin + EditorGUIUtility.singleLineHeight;
            label = EditorGUI.BeginProperty(position, 
                new GUIContent(allowedIdnexesProperty.displayName), 
                allowedIdnexesProperty);
            allowedIdnexesProperty.isExpanded = EditorGUI.Foldout(position, allowedIdnexesProperty.isExpanded, label, true);
            if (allowedIdnexesProperty.isExpanded)
            {
                var enterChildren = true;
                //cache all needed property references
                var targetProperty = allowedIdnexesProperty.Copy();
                var endingProperty = allowedIdnexesProperty.GetEndProperty();

                EditorGUI.indentLevel++;
                //iterate over all children (but only 1 level depth)
                while (targetProperty.NextVisible(enterChildren))
                {
                    if (SerializedProperty.EqualContents(targetProperty, endingProperty))
                    {
                        break;
                    }

                    PreventActions();
                    position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    using (new EditorGUI.DisabledScope(enterChildren))
                    {
                        var element = targetProperty.Copy();
                        EditorGUI.PropertyField(position, element);
                    }

                    enterChildren = false;
                }

                EditorGUI.indentLevel--;
            }


            EditorGUI.EndProperty();
            EditorGUI.indentLevel--;
        }
    }
}