using System;
using System.Collections.Generic;
using UnityEngine;

namespace Toolbox.Lighting
{
    /// <summary>
    /// Class responsible for blending multiple lightmaps into one.
    /// </summary>
    [Serializable]
    public class LightmapTransitionPreset : IDisposable
    {
        private readonly List<RenderTexture> shadowMasks = new List<RenderTexture>();
        private readonly List<RenderTexture> lightmapDirs = new List<RenderTexture>();
        private readonly List<RenderTexture> lightmapColors = new List<RenderTexture>();

        private readonly Material blendingMaterial;

        private readonly LightmapPreset[] presets;

        private Dictionary<string, LightmapPreset> mappedPresets;

        private LightmapRuntimePreset runtimePreset;

        private float lastBlendValue;

        private bool isDirty;

        private LightmapPreset[] presetsToBlend;

        [SerializeField]
        private bool[] allowedIndexes;

        /// <summary>
        /// Used to speed-up checks.
        /// </summary>
        private Dictionary<LightmapPreset, int> mappedBlendedPresets;


        internal int PresetsToBlendCount { get; private set; }


        internal LightmapData[] Lightmaps
        {
            get => runtimePreset.TargetPreset.Lightmaps;
        }
        
        internal LightProbes LightProbes
        {
            get => runtimePreset.TargetPreset.LightProbes;
        }


        internal LightmapTransitionPreset(LightmapPreset[] presets)
            : this(presets, new Material(Shader.Find("Hidden/Lightmap Blend")))
        { }

        internal LightmapTransitionPreset(LightmapPreset[] presets, Material blendingMaterial)
        {
            if (presets == null || presets.Length == 0)
            {
                throw new ArgumentException(nameof(presets));
            }

            this.blendingMaterial = blendingMaterial;

            shadowMasks = new List<RenderTexture>();
            lightmapDirs = new List<RenderTexture>();
            lightmapColors = new List<RenderTexture>();

            this.presets = presets;
            PrepareStaticPresets();
            PrepareRuntimePreset();
        }


        private void PrepareStaticPresets()
        {
            mappedPresets = new Dictionary<string, LightmapPreset>(presets.Length);
            foreach (var preset in presets)
            {
                mappedPresets.Add(preset.PresetName, preset);
            }
        }

        private void PrepareRuntimePreset()
        {
            var mockupPreset = presets[0];
            runtimePreset = new LightmapRuntimePreset(mockupPreset);
            var texturesSets = runtimePreset.TargetPreset.TexturesSets;
            allowedIndexes = new bool[texturesSets.Length];

            for (var i = 0; i < texturesSets.Length; i++)
            {
                var texturesSet = texturesSets[i];
                shadowMasks.Add(CreateTransitionTexture(texturesSet.shadowMask));
                lightmapDirs.Add(CreateTransitionTexture(texturesSet.lightmapDir));
                lightmapColors.Add(CreateTransitionTexture(texturesSet.lightmapColor));
                allowedIndexes[i] = true;
            }
        }

        private RenderTexture CreateTransitionTexture(Texture2D texture)
        {
            if (!texture)
            {
                return null;
            }

            var renderTexture = new RenderTexture(texture.width, texture.height, 0)
            {
                useMipMap = true,
                graphicsFormat = texture.graphicsFormat
            };
            return renderTexture;
        }


        internal bool Update(float blendValue)
        {
            if (Mathf.Approximately(lastBlendValue, blendValue) && !isDirty)
            {
                return false;
            }

            var step = 1.0f / (PresetsToBlendCount - 1);
            var indexA = Mathf.Min((int)(blendValue / step), PresetsToBlendCount - 2);
            var indexB = indexA + 1;
            var realBlendValue = (blendValue - indexA * step) / step;

            //set current blending value (between 0 and 1)
            blendingMaterial.SetFloat("_Blend", realBlendValue);

            var targetLightmaps = Lightmaps;
            var lightmapsCount = targetLightmaps.Length;
            var lightmapsA = presetsToBlend[indexA].Lightmaps;
            var lightmapsB = presetsToBlend[indexB].Lightmaps;
            for (var i = 0; i < lightmapsCount; i++)
            {
                if (!allowedIndexes[i])
                {
                    continue;
                }

                var target = targetLightmaps[i];
                var lightmapA = lightmapsA[i];
                var lightmapB = lightmapsB[i];

                const string blendTextureName = "_BlendTex";
                if (shadowMasks[i])
                {
                    //blend shadowMask texture at index
                    blendingMaterial.SetTexture(blendTextureName, lightmapB.shadowMask);
                    Graphics.Blit(lightmapA.shadowMask, shadowMasks[i], blendingMaterial);
                    Graphics.CopyTexture(shadowMasks[i], target.shadowMask);
                }

                if (lightmapDirs[i])
                {
                    //blend directional texture at index
                    blendingMaterial.SetTexture(blendTextureName, lightmapB.lightmapDir);
                    Graphics.Blit(lightmapA.lightmapDir, lightmapDirs[i], blendingMaterial);
                    Graphics.CopyTexture(lightmapDirs[i], target.lightmapDir);
                }

                if (lightmapColors[i])
                {
                    //blend color texture at index
                    blendingMaterial.SetTexture(blendTextureName, lightmapB.lightmapColor);
                    Graphics.Blit(lightmapA.lightmapColor, lightmapColors[i], blendingMaterial);
                    Graphics.CopyTexture(lightmapColors[i], target.lightmapColor);
                }
            }

            lastBlendValue = blendValue;
            isDirty = false;
            return true;
        }

        internal void SetPresetsToBlend(params LightmapPreset[] presetsToBlend)
        {
            if (presetsToBlend.Length < 2)
            {
                throw new ArgumentException(nameof(presetsToBlend));
            }

            //TODO: presets validation?
            this.presetsToBlend = presetsToBlend;
            PresetsToBlendCount = presetsToBlend.Length;
            mappedBlendedPresets = new Dictionary<LightmapPreset, int>(PresetsToBlendCount);
            LightmapPreset preset;
            for (var i = 0; i < PresetsToBlendCount; i++)
            {
                preset = presetsToBlend[i];
                if (mappedBlendedPresets.ContainsKey(preset))
                {
                    continue;
                }
                else
                {
                    mappedBlendedPresets.Add(preset, i);
                }
            }

            isDirty = true;
        }

        internal void SetAllowedIndexes(bool[] allowedIndexes)
        {
            if (allowedIndexes == null)
            {
                throw new ArgumentNullException(nameof(allowedIndexes));
            }

            if (allowedIndexes.Length != PresetsToBlendCount)
            {
                throw new ArgumentException(nameof(allowedIndexes));
            }

            this.allowedIndexes = allowedIndexes;
        }

        internal bool IsPresetBlended(LightmapPreset preset, out int order)
        {
            order = -1;
            if (mappedBlendedPresets == null)
            {
                return false;
            }

            return mappedBlendedPresets.TryGetValue(preset, out order);
        }

        internal void SetIndexAllowed(int index, bool allowed)
        {
            allowedIndexes[index] = allowed;
        }

        public void Dispose()
        {
            if (runtimePreset == null)
            {
                return;
            }

            runtimePreset.Dispose();
            foreach (var shadowMask in shadowMasks)
            {
                LightmappingManager.SafeObjectDestroy(shadowMask);
            }

            foreach (var lightmapDir in lightmapDirs)
            {
                LightmappingManager.SafeObjectDestroy(lightmapDir);
            }

            foreach (var lightmapColor in lightmapColors)
            {
                LightmappingManager.SafeObjectDestroy(lightmapColor);
            }

            shadowMasks.Clear();
            lightmapDirs.Clear();
            lightmapColors.Clear();

            LightmappingManager.SafeObjectDestroy(blendingMaterial);
        }
    }
}