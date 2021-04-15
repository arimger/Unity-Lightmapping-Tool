using UnityEngine;
using UnityEditor;

namespace Toolbox.Lighting.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(LightmapPreset))]
    internal class LightmapPresetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            using (new EditorGUILayout.VerticalScope(Style.sectionStyle))
            {
                EditorGUILayout.LabelField("Actions", Style.headerStyle);

                using (new EditorGUI.DisabledGroupScope(true))
                {
                    if (GUILayout.Button("Fill"))
                    {
                        //TODO:
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.TextField("Directory", string.Empty);
                        if (GUILayout.Button(Style.directoryButtonLabel, Style.directoryButtonOptions))
                        {
                            //TODO
                        }
                    }
                }
            }
        }


        private static class Style
        {
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