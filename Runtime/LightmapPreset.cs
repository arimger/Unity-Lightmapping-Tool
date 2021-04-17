using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Toolbox.Lighting
{
    [CreateAssetMenu(fileName = "Lightmap Preset", menuName = "Toolbox/Lighting/Lightmap Preset")]
    public class LightmapPreset : ScriptableObject
    {
        [SerializeField, Tooltip("Name of the lightmap preset, should be unique.")]
        private string presetName;
        [SerializeField]
        private string directory;

        [SerializeField, NonReorderable]
        private LightmapTexturesSet[] texturesSets;

        [SerializeField, NonReorderable]
        private Texture[] reflectionProbes;

        [SerializeField]
        private LightProbes lightProbes;

        private LightmapData[] lightmaps;


        public string PresetName
        {
            get => presetName;
            set => presetName = value;
        }

        public LightmapTexturesSet[] TexturesSets
        {
            get => texturesSets;
            set => texturesSets = value;
        }

        public Texture[] ReflectionProbes
        {
            get => reflectionProbes;
            set => reflectionProbes = value;
        }

        public LightProbes LightProbes
        {
            get => lightProbes;
            set => lightProbes = value;
        }

        public LightmapData[] Lightmaps
        {
            get
            {
                if (lightmaps == null)
                {
                    CreateLightmapData();
                }

                return lightmaps;
            }
        }


        private bool CreateLightmapData()
        {
            if (texturesSets == null || texturesSets.Length == 0)
            {
                return false;
            }

            lightmaps = new LightmapData[texturesSets.Length];
            for (var i = 0; i < lightmaps.Length; i++)
            {
                var texturesSet = texturesSets[i];
                lightmaps[i] = new LightmapData()
                {
                    shadowMask = texturesSet.shadowMask,
                    lightmapDir = texturesSet.lightmapDir,
                    lightmapColor = texturesSet.lightmapColor,
                };
            }

            return true;
        }

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
            TexturesSets = texturesSets;
        }

        private void LoadCubemaps(IReadOnlyList<Cubemap> cubemaps)
        {
            var reflectionProbes = new Texture[cubemaps.Count];
            for (var i = 0; i < cubemaps.Count; i++)
            {                
                //TODO: validation
                reflectionProbes[i] = cubemaps[i];
            }

            ReflectionProbes = reflectionProbes;
        }

        private void LoadLightingData(LightingDataAsset lightingData)
        {
#if UNITY_EDITOR
            var path = AssetDatabase.GetAssetPath(lightingData);
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var asset in assets)
            {
                if (asset is LightProbes lightProbes)
                {
                    this.lightProbes = lightProbes;
                    return;
                }
            }
#endif
        }


        public void LoadPresetFromDirectory()
        {
            LoadPresetFromDirectory(directory);
        }

        public void LoadPresetFromDirectory(string directory)
        {
#if UNITY_EDITOR
            var relativePath = directory.Replace(Application.dataPath, "Assets");
            if (!Directory.Exists(directory) || !AssetDatabase.IsValidFolder(relativePath))
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
#else
            InternalLogger.Log(LogType.Error, "Presets can be loaded only in the Editor.");
#endif
        }


        private static class Defaults
        {
            internal static readonly string shadowMaskSuffix = "shadowmask";
            internal static readonly string lightmapDirSuffix = "dir";
            internal static readonly string lightmapColorSuffix = "light";
        }
    }
}