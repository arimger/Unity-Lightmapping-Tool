using UnityEditor;
using UnityEngine;

namespace Toolbox.Lighting.Editor
{
    [CustomPropertyDrawer(typeof(LightmapTexturesSet))]
    public class LightmapTexturesSetDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var shadowMaskProperty = property.FindPropertyRelative("shadowMask");
            var lightmapDirProperty = property.FindPropertyRelative("lightmapDir");
            var lightmapColorProperty = property.FindPropertyRelative("lightmapColor");

            var realPosition = EditorGUI.IndentedRect(position);
            var indentWidth = position.width - realPosition.width;
            var fullWidth = realPosition.width;
            var propertyWidth = fullWidth / 3;
            var labelWidth = EditorGUIUtility.labelWidth;
            position.xMax = position.xMin + propertyWidth;
            EditorStyles.label.CalcMinMaxWidth(Style.shadowMaskContent, out _, out var maxWidth);
            EditorGUIUtility.labelWidth = maxWidth + indentWidth;
            EditorGUI.PropertyField(position, lightmapColorProperty, Style.lightmapColorContent);
            position.x += propertyWidth;
            EditorGUI.PropertyField(position, lightmapDirProperty, Style.lightmapDirContent);
            position.x += propertyWidth;
            EditorGUI.PropertyField(position, shadowMaskProperty, Style.shadowMaskContent);
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUI.EndProperty();
        }


        private static class Style
        {
            internal static readonly GUIContent shadowMaskContent = new GUIContent("Mask", "ShadowMask");
            internal static readonly GUIContent lightmapDirContent = new GUIContent("Dir", "Directional Lightmap");
            internal static readonly GUIContent lightmapColorContent = new GUIContent("Col", "Color Lightmap");
        }
    }
}