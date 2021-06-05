using UnityEditor;
using UnityEngine;

namespace Toolbox.Lighting.Editor
{
    [CustomPropertyDrawer(typeof(LightmapTransitionPreset))]
    internal class LightmapTransitionPresetDrawer : PropertyDrawer
    {
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
            var lastBlendValueProperty = property.FindPropertyRelative("lastBlendValue");

            var indexA = -1;
            var indexB = -1;
            var presetsCount = blendedPresetsProperty.arraySize;
            if (presetsCount >= LightmapTransitionPreset.minimalPresetsToBlend)
            {
                var blendValue = lastBlendValueProperty.floatValue;
                var step = 1.0f / (presetsCount - 1);
                indexA = Mathf.Min((int)(blendValue / step), presetsCount - 2);
                indexB = indexA + 1;
            }

            position.yMax = position.yMin + EditorGUIUtility.singleLineHeight;
            EditorGUI.indentLevel++;
            EditorHelper.DrawNativeList(ref position, blendedPresetsProperty, true, true, (rect, element, index, label) =>
            {
                //additionally mark currently blended presets
                if (index == indexA || index == indexB)
                {
                    label = new GUIContent($"{label.text} [Blend]");
                }

                //prevent all actions over this preset element
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUI.PropertyField(rect, element, label, element.isExpanded);
                }
            }, "Position");
            EditorHelper.DrawNativeList(ref position, allowedIdnexesProperty, true, true, (rect, element, index, label) =>
            {
                EditorGUI.PropertyField(rect, element, label, element.isExpanded);
            }, "Index");
            EditorGUI.indentLevel--;
        }
    }
}