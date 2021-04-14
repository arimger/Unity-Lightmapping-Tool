using UnityEngine;
using UnityEditor;

namespace Toolbox.Lighting.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(LightmappingManager))]
    internal class LightmappingManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            using (new EditorGUILayout.VerticalScope(Style.settingsGroupStyle))
            {
                EditorGUILayout.LabelField("Settings", Style.settingsHeaderStyle);
                EditorGUILayout.TextField("");
                EditorGUILayout.TextField("");
                if (GUILayout.Button("TEST"))
                {

                }
            }
        }


        private static class Style
        {
            internal static readonly GUIStyle settingsHeaderStyle = new GUIStyle(EditorStyles.boldLabel);
            internal static readonly GUIStyle settingsGroupStyle = new GUIStyle(EditorStyles.helpBox);
        }
    }
}