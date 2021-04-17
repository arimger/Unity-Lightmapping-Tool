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
        private SerializedProperty presetsProperty;
        private SerializedProperty probesProperty;
        private SerializedProperty transitionPresetProperty;
        private SerializedProperty blendValueProperty;

        private ReorderableList presetsList;
        private ReorderableList probesList;

        private LightmapPreset[] presetsToBlend;
        private LightmapPreset presetToSwitch;


        private void OnEnable()
        {
            manager = target as LightmappingManager;

            presetsToBlend = new LightmapPreset[0];

            currentModeProperty = serializedObject.FindProperty("currentMode");
            initOnAwakeProperty = serializedObject.FindProperty("initOnAwake");
            presetsProperty = serializedObject.FindProperty("presets");
            probesProperty = serializedObject.FindProperty("probes");
            transitionPresetProperty = serializedObject.FindProperty("transitionPreset");
            blendValueProperty = serializedObject.FindProperty("blendValue");

            presetsList = EditorHelper.CreateList(presetsProperty);
            presetsList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = presetsProperty.GetArrayElementAtIndex(index);
                if (manager.IsInitialized)
                {
                    if (element.objectReferenceValue is LightmapPreset preset)
                    {
                        if (manager.IsPresetBlended(preset, out var order))
                        {
                            var label = $"Blend [{order}]";
                            EditorGUI.LabelField(rect, label);
                            rect.xMin += Style.presetBlendInfoPadding;
                        }
                    }
                }

                EditorGUI.PropertyField(rect, element, GUIContent.none, element.isExpanded);
            };
            probesList = EditorHelper.CreateList(probesProperty);
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
                        manager.SwitchLightmap(presetToSwitch);
                    }
                }

                presetToSwitch = EditorGUILayout.ObjectField(presetToSwitch, typeof(LightmapPreset), false) as LightmapPreset;
            }
        }

        private void DrawBlendingMode()
        {
            EditorGUILayout.Space();
            using (new EditorGUILayout.VerticalScope(Style.sectionStyle))
            {
                EditorGUILayout.LabelField(Style.blendingModeHeaderContent, Style.headerStyle);
                if (presetsList.count == 0)
                {
                    EditorGUILayout.HelpBox(Style.blendingPresetsAreEmpty.text, MessageType.Warning);
                }

                presetsList.DoLayoutList();
                EditorGUILayout.Space();
                probesList.DoLayoutList();
                EditorGUILayout.PropertyField(transitionPresetProperty);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(blendValueProperty);
            }

            using (new EditorGUILayout.VerticalScope(Style.sectionStyle))
            {
                EditorGUILayout.LabelField("Actions", Style.headerStyle);

                using (new EditorGUI.DisabledGroupScope(!manager.IsInitialized))
                {
                    if (GUILayout.Button("Set Presets To Blend"))
                    {
                        if (presetsToBlend.Length > 1)
                        {
                            manager.SetPresetsToBlend(presetsToBlend);
                        }
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

                EditorGUILayout.Space();
                if (GUILayout.Button("Search For Probes"))
                {
                    if (EditorUtility.DisplayDialog(string.Empty,
                        "Do you want to search for ReflectionProbes and replace the current list?", 
                        "Yes", "Cancel"))
                    {
                        manager.SearchForReflectionProbes();
                    }
                }
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
                if (manager.IsInitialized)
                {
                    manager.Initialize((LightmappingManager.Mode)currentModeProperty.intValue);
                }
            }

            var isInitialized = manager.IsInitialized;
            using (new EditorGUI.DisabledScope(isInitialized))
            {
                EditorGUI.BeginChangeCheck();
                isInitialized = EditorGUILayout.Toggle("Is Initialized", isInitialized);
                if (EditorGUI.EndChangeCheck())
                {
                    if (isInitialized)
                    {
                        manager.Initialize(manager.CurrentMode);
                    }
                }
            }

            EditorGUILayout.PropertyField(initOnAwakeProperty);
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

            internal static readonly GUIContent blendingPresetsAreEmpty = new GUIContent("Presets list is empty!");
            internal static readonly GUIContent managerIsDisabledContent = new GUIContent("Manager is disabled!");
            internal static readonly GUIContent blendingModeHeaderContent = new GUIContent("Content");

            internal static readonly GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            internal static readonly GUIStyle sectionStyle = new GUIStyle(EditorStyles.helpBox);
        }
    }
}