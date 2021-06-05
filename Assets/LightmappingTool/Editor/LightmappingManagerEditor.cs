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
        private SerializedProperty cachedProbesProperty;
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
            cachedProbesProperty = serializedObject.FindProperty("cachedProbes");
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
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(cachedProbesProperty);
            EditorGUILayout.Space();
            using (new EditorGUILayout.VerticalScope(Style.sectionStyle))
            {
                EditorGUILayout.LabelField("Actions", Style.headerStyle);
                if (GUILayout.Button("Search For Probes"))
                {
                    if (EditorUtility.DisplayDialog(string.Empty,
                        "Do you want to search for ReflectionProbes and replace the current list?",
                        "Yes", "Cancel"))
                    {
                        manager.SearchForReflectionProbes();
                    }
                }

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
                    if (manager.IsAbleToBlend)
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
                        EditorGUILayout.HelpBox("Set presets to re-initialize mode", MessageType.Warning);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Not allowed to work in the Edit mode", MessageType.Warning);
                }

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(blendValueProperty);
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                DrawProgressBar();
            }

            using (new EditorGUILayout.VerticalScope(Style.sectionStyle))
            {
                EditorGUILayout.LabelField(Style.actionsSectionHeaderContent, Style.headerStyle);
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
            }
        }

        private void DrawProgressBar()
        {
            var presetsCount = manager.PresetsToBlendCount;
            if (presetsCount < LightmapTransitionPreset.minimalPresetsToBlend)
            {
                return;
            }

            var rect = GUILayoutUtility.GetRect(0, Style.progressBarHeight);
            var fillValue = blendValueProperty.floatValue;
            var fillRect = new Rect(rect.x + Style.fillRectOffset / 2,
                                    rect.y + Style.fillRectOffset / 2,
                                    (rect.width - Style.fillRectOffset) * fillValue,
                                    rect.height - Style.fillRectOffset);

            //create background rect to fill with color
            if (Event.current.type == EventType.Repaint)
            {
                Style.backgroundStyle.Draw(rect, false, false, false, false);
            }

            //create fill rect based on fill value
            EditorGUI.DrawRect(fillRect, Style.fillRectColor);

            var step = 1.0f / (presetsCount - 1);
            //add separators based on internal presets count
            var markerRect = new Rect(rect);
            markerRect.xMax = markerRect.xMin + Style.fillSeparatorWidth;
            var widthStep = rect.width * step;
            for (var i = 1; i < presetsCount - 1; i++)
            {
                markerRect.x += widthStep;
                EditorGUI.DrawRect(markerRect, Style.separatorColor);
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
            internal static readonly float progressBarHeight = 18.0f;
            internal static readonly float fillRectOffset = 4.0f;
            internal static readonly float fillSeparatorWidth = 2.0f;

            internal static readonly Color fillRectColor = new Color(0.9f, 0.93f, 0.6f);
            internal static readonly Color separatorColor = EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.65f, 0.65f, 0.65f);

            internal static readonly GUIContent presetsAreEmptyContent = new GUIContent("Presets list is empty!");
            internal static readonly GUIContent managerIsDisabledContent = new GUIContent("Manager is disabled!");
            internal static readonly GUIContent blendingModeHeaderContent = new GUIContent("Blending");
            internal static readonly GUIContent actionsSectionHeaderContent = new GUIContent("Actions");

            internal static readonly GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            internal static readonly GUIStyle sectionStyle = new GUIStyle(EditorStyles.helpBox);
            internal static readonly GUIStyle backgroundStyle = new GUIStyle(EditorStyles.helpBox);
        }
    }
}