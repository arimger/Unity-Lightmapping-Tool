using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Toolbox.Lighting.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(LightmapPreset))]
    internal class LightmapPresetEditor : Editor
    {
        private string pickedDirectory;


        private int GetAssetIndexFromName(string name)
        {
            return int.Parse(Regex.Match(name, @"\d+").Value);
        }

        private void LoadTextures(IReadOnlyList<Texture2D> textures)
        {
            var highestIndex = 0;
            var texturesSets = new LightmapTexturesSet[textures.Count];
            foreach (var texture in textures)
            {
                var name = texture.name;
                var index = GetAssetIndexFromName(name);
                if (index > highestIndex)
                {
                    highestIndex = index;
                }

                if (texturesSets[index] == null)
                {
                    texturesSets[index] = new LightmapTexturesSet();
                }

                //TODO: cleaner way
                if (name.EndsWith(Defaults.shadowMaskSuffix))
                {
                    texturesSets[index].shadowMask = texture;
                }
                else if (name.EndsWith(Defaults.lightmapDirSuffix))
                {
                    texturesSets[index].lightmapDir = texture;
                }
                else if (name.EndsWith(Defaults.lightmapColorSuffix))
                {
                    texturesSets[index].lightmapColor = texture;
                }
            }

            Array.Resize(ref texturesSets, highestIndex + 1);
            var preset = target as LightmapPreset;
            preset.TexturesSets = texturesSets;
        }

        private void LoadCubemaps(IReadOnlyList<Cubemap> cubemaps)
        {
            foreach (var cubemap in cubemaps)
            {
                var index = GetAssetIndexFromName(cubemap.name);
            }
        }

        private void LoadLightingData(LightingDataAsset lightingData)
        {

        }

        private void LoadPresetWithFiles()
        {
            var relativePath = pickedDirectory.Replace(Application.dataPath, "Assets");
            if (!Directory.Exists(pickedDirectory) || !AssetDatabase.IsValidFolder(relativePath))
            {
                InternalLogger.Log(LogType.Warning, "Picked directory is invalid.");
                return;
            }

            var assetGuids = AssetDatabase.FindAssets("", new[] { relativePath });
            if (assetGuids == null || assetGuids.Length == 0)
            {
                InternalLogger.Log(LogType.Warning, $"No assets found in the given directory ({relativePath}).");
            }

            var lightmapTextures = new List<Texture2D>();
            var lightmapCubemaps = new List<Cubemap>();
            LightingDataAsset lightmapData = null;

            for (var i = 0; i < assetGuids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                var asset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                switch (asset)
                {
                    case Texture2D texture:
                        lightmapTextures.Add(texture);
                        break;
                    case Cubemap cubemap:
                        lightmapCubemaps.Add(cubemap);
                        break;
                    case LightingDataAsset data:
                        lightmapData = data;
                        //TODO:
                        //var assets = AssetDatabase.LoadAllAssetsAtPath(relativePath);
                        break;
                }
            }

            if (lightmapData == null)
            {
                InternalLogger.Log(LogType.Warning, $"No {nameof(LightingDataAsset)} found in the given directory ({relativePath}).");
                return;
            }

            LoadTextures(lightmapTextures);
            LoadCubemaps(lightmapCubemaps);
            LoadLightingData(lightmapData);
            InternalLogger.Log(LogType.Log, "Preset loaded.");
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            EditorGUILayout.Space();
            using (new EditorGUILayout.VerticalScope(Style.sectionStyle))
            {
                EditorGUILayout.LabelField("Actions", Style.headerStyle);

                if (GUILayout.Button(Style.loadSceneButtonLabel))
                {
                    LoadPresetWithFiles();
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    pickedDirectory = EditorGUILayout.TextField("Directory", pickedDirectory);
                    if (GUILayout.Button(Style.directoryButtonLabel, Style.directoryButtonOptions))
                    {
                        pickedDirectory = EditorUtility.OpenFolderPanel("Open Lightmap directory", "Assets", string.Empty);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }


        private static class Defaults
        {
            internal static readonly string shadowMaskSuffix = "mask";
            internal static readonly string lightmapDirSuffix = "dir";
            internal static readonly string lightmapColorSuffix = "light";
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