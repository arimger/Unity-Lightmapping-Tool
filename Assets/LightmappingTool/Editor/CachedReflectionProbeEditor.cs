using System;
using UnityEngine;
using UnityEditor;

namespace Toolbox.Lighting.Editor
{
    using Editor = UnityEditor.Editor;

    [Obsolete]
    [CustomEditor(typeof(CachedReflectionProbe))]
    internal class CachedReflectionProbeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var component = target as CachedReflectionProbe;
            if (component.ReflectionProbe)
            {
                var probe = component.ReflectionProbe;
                EditorGUILayout.ObjectField(probe.bakedTexture, typeof(Texture), true);
            }
        }
    }
}