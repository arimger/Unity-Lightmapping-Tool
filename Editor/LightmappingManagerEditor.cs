using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Toolbox.Lighting.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(LightmappingManager))]
    internal class LightmappingManagerEditor : Editor
    {
        private LightmappingManager manager;

        private SerializedProperty currentModeProperty;
        private SerializedProperty initOnAwakeProperty;
        private SerializedProperty useEditModeProperty;
        private SerializedProperty initialPresetsProperty;
        private SerializedProperty blendingPresetProperty;
        private SerializedProperty blendValueProperty;

        private ReorderableList initialPresetsList;

        /// <summary>
        /// An array of cached presets that can be used for the "SetPresetsToBlend" action.
        /// </summary>
        private LightmapPreset[] presetsToBlend;

        /// <summary>
        /// Cached preset that can be used for the "Switch" action.
        /// </summary>
        private LightmapPreset presetToSwitch;


        private void OnEnable()
        {
            manager = target as LightmappingManager;

            presetsToBlend = new LightmapPreset[0];

            currentModeProperty = serializedObject.FindProperty("currentMode");
            initOnAwakeProperty = serializedObject.FindProperty("initOnAwake");
            useEditModeProperty = serializedObject.FindProperty("useEditMode");
            initialPresetsProperty = serializedObject.FindProperty("initialPresets");
            blendingPresetProperty = serializedObject.FindProperty("blendingPreset");
            blendValueProperty = serializedObject.FindProperty("blendValue");

            initialPresetsList = EditorHelper.CreateList(initialPresetsProperty);
            initialPresetsList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = initialPresetsProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, element, GUIContent.none, element.isExpanded);
            };
        }


        private void DrawSwitcherMode()
        {
            using (new EditorGUILayout.VerticalScope(Style.sectionStyle))
            {
                EditorGUILayout.LabelField("Actions", Style.headerStyle);
                using (new EditorGUI.DisabledScope(presetToSwitch == null))
                {
                    if (GUILayout.Button("Switch Lightmap"))
                    {
                        manager.SwitchLightmaps(presetToSwitch);
                    }
                }

                presetToSwitch = EditorGUILayout.ObjectField(presetToSwitch, typeof(LightmapPreset), false) as LightmapPreset;
            }
        }

        private void DrawBlendingMode()
        {
            EditorGUILayout.PropertyField(useEditModeProperty);
            EditorGUILayout.PropertyField(initOnAwakeProperty);

            if (initOnAwakeProperty.boolValue)
            {
                if (initialPresetsList.count == 0)
                {
                    EditorGUILayout.HelpBox(Style.presetsAreEmptyContent.text, MessageType.Warning);
                }

                initialPresetsList.DoLayoutList();
                EditorGUILayout.Space();
            }

            using (new EditorGUILayout.VerticalScope(Style.sectionStyle))
            {
                EditorGUILayout.LabelField(Style.blendingModeHeaderContent, Style.headerStyle);

                if (manager.IsAbleToWork)
                {
                    if (manager.PresetsToBlendCount == 0)
                    {
                        EditorGUILayout.HelpBox("Not enough presets to blend", MessageType.Warning);
                    }


                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(blendingPresetProperty);
                }
                else
                {
                    EditorGUILayout.HelpBox("Not allowed to work in the Edit mode", MessageType.Warning);
                }

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(blendValueProperty);
            }

            using (new EditorGUILayout.VerticalScope(Style.sectionStyle))
            {
                EditorGUILayout.LabelField("Actions", Style.headerStyle);
                EditorGUILayout.Space();
                using (new EditorGUI.DisabledGroupScope(false))
                {
                    if (GUILayout.Button("Set Presets To Blend"))
                    {
                        manager.SetPresetsToBlend(presetsToBlend);
                    }

                    var size = presetsToBlend.Length;
                    EditorGUI.BeginChangeCheck();
                    size = EditorGUILayout.IntField(size);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (size != presetsToBlend.Length)
                        {
                            Array.Resize(ref presetsToBlend, size);
                        }
                    }

                    for (var i = 0; i < size; i++)
                    {
                        presetsToBlend[i] = EditorGUILayout.ObjectField(presetsToBlend[i], typeof(LightmapPreset), false) as LightmapPreset;
                    }
                }

                //TODO: restore reflection probes-related actions
                //EditorGUILayout.Space();
                //if (GUILayout.Button("Search For Probes"))
                //{
                //    if (EditorUtility.DisplayDialog(string.Empty,
                //        "Do you want to search for ReflectionProbes and replace the current list?",
                //        "Yes", "Cancel"))
                //    {
                //        manager.SearchForReflectionProbes();
                //    }
                //}
            }
        }


        public override void OnInspectorGUI()
        {
            if (!manager.enabled)
            {
                EditorGUILayout.HelpBox(Style.managerIsDisabledContent.text, MessageType.Warning);
            }

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(currentModeProperty);
            if (EditorGUI.EndChangeCheck())
            {
                manager.ChangeMode((LightmappingManager.Mode)currentModeProperty.intValue);
            }

            switch (currentModeProperty.intValue)
            {
                case 0:
                    DrawSwitcherMode();
                    break;
                case 1:
                    DrawBlendingMode();
                    break;
                default:
                    //???
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }


        private static class Style
        {
            internal static readonly float presetBlendInfoPadding = 65.0f;

            internal static readonly GUIContent presetsAreEmptyContent = new GUIContent("Presets list is empty!");
            internal static readonly GUIContent managerIsDisabledContent = new GUIContent("Manager is disabled!");
            internal static readonly GUIContent blendingModeHeaderContent = new GUIContent("Blending");

            internal static readonly GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            internal static readonly GUIStyle sectionStyle = new GUIStyle(EditorStyles.helpBox);
        }
    }
}