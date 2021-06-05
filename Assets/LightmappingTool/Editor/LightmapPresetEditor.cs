using UnityEngine;
using UnityEditor;

namespace Toolbox.Lighting.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(LightmapPreset))]
    internal class LightmapPresetEditor : Editor
    {
        private SerializedProperty presetNameProperty;
        private SerializedProperty directoryProperty;
        private SerializedProperty texturesSetsProperty;
        private SerializedProperty reflectionProbesProperty;
        private SerializedProperty lightProbesProperty;


        private void OnEnable()
        {
            presetNameProperty = serializedObject.FindProperty("presetName");
            directoryProperty = serializedObject.FindProperty("directory");
            texturesSetsProperty = serializedObject.FindProperty("texturesSets");
            reflectionProbesProperty = serializedObject.FindProperty("reflectionProbes");
            lightProbesProperty = serializedObject.FindProperty("lightProbes");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(presetNameProperty);
            EditorGUILayout.Space();
            using (new EditorGUILayout.VerticalScope(Style.sectionStyle))
            {
                EditorGUILayout.LabelField("Lightmap", Style.headerStyle);
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(texturesSetsProperty);
                EditorHelper.DrawNativeList(reflectionProbesProperty, true, false, (element, index, label) =>
                {
                    EditorGUILayout.PropertyField(element, GUIContent.none);
                });
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                EditorGUILayout.Toggle("Light Probes loaded", lightProbesProperty.objectReferenceValue);
            }

            EditorGUILayout.Space();
            using (new EditorGUILayout.VerticalScope(Style.sectionStyle))
            {
                EditorGUILayout.LabelField("Actions", Style.headerStyle);
                EditorGUILayout.Space();
                if (GUILayout.Button(Style.loadSceneButtonLabel))
                {
                    var preset = target as LightmapPreset;
                    preset.LoadPresetFromDirectory();
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(directoryProperty);
                    if (GUILayout.Button(Style.directoryButtonLabel, Style.directoryButtonOptions))
                    {
                        directoryProperty.stringValue = EditorUtility.OpenFolderPanel("Open Lightmap directory", "Assets", string.Empty);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }


        private static class Style
        {
            internal static readonly GUIContent loadSceneButtonLabel = new GUIContent("Load", "Try to fill this preset using given directory.");
            internal static readonly GUIContent directoryButtonLabel = new GUIContent(EditorGUIUtility.FindTexture("Folder Icon"), "Pick directory");

            internal static readonly GUILayoutOption[] directoryButtonOptions = new GUILayoutOption[]
            {
                GUILayout.Width(30.0f),
                GUILayout.Height(18.0f)
            };

            internal static readonly GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            internal static readonly GUIStyle sectionStyle = new GUIStyle(EditorStyles.helpBox);
        }
    }
}