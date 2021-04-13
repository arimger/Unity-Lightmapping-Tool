using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Toolbox.Lighting
{
    /// <summary>
    /// Class responsible for blending multiple lightmaps into one.
    /// </summary>
    internal class LightmapTransitionPreset : IDisposable
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

        private LightmapPreset presetA;
        private LightmapPreset presetB;

        private HashSet<int> allowedIndexes;


        internal LightmapData[] Lightmaps
        {
            get => runtimePreset.TargetPreset.Lightmaps;
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
                mappedPresets.Add(preset.LightingName, preset);
            }
        }

        private void PrepareRuntimePreset()
        {
            var mockupPreset = presets[0];
            runtimePreset = new LightmapRuntimePreset(mockupPreset);
            var texturesSets = runtimePreset.TargetPreset.TexturesSets;
            allowedIndexes = new HashSet<int>();

            for (var i = 0; i < texturesSets.Length; i++)
            {
                var texturesSet = texturesSets[i];
                shadowMasks.Add(CreateTransitionTexture(texturesSet.shadowMask));
                lightmapDirs.Add(CreateTransitionTexture(texturesSet.lightmapDir));
                lightmapColors.Add(CreateTransitionTexture(texturesSet.lightmapColor));
                allowedIndexes.Add(i);
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
            if (lastBlendValue == blendValue && !isDirty)
            {
                return false;
            }

            //set current blending value (between 0 and 1)
            blendingMaterial.SetFloat("_Blend", blendValue);

            var targetLightmaps = Lightmaps;
            var lightmapsCount = targetLightmaps.Length;
            var lightmapsA = presetA.Lightmaps;
            var lightmapsB = presetB.Lightmaps;
            for (var i = 0; i < lightmapsCount; i++)
            {
                if (!allowedIndexes.Contains(i))
                {
                    continue;
                }

                var target = targetLightmaps[i];
                var lightmapA = lightmapsA[i];
                var lightmapB = lightmapsB[i];

                //blend shadowMask texture at index
                //blendingMaterial.SetTexture("_BlendTex", lightmapB.shadowMask);
                //Graphics.Blit(lightmapA.shadowMask, shadowMasks[i], blendingMaterial);
                //Graphics.CopyTexture(shadowMasks[i], target.shadowMask);

                //blend directional texture at index
                blendingMaterial.SetTexture("_BlendTex", lightmapB.lightmapDir);
                Graphics.Blit(lightmapA.lightmapDir, lightmapDirs[i], blendingMaterial);
                Graphics.CopyTexture(lightmapDirs[i], target.lightmapDir);

                //blend color texture at index
                blendingMaterial.SetTexture("_BlendTex", lightmapB.lightmapColor);
                Graphics.Blit(lightmapA.lightmapColor, lightmapColors[i], blendingMaterial);
                Graphics.CopyTexture(lightmapColors[i], target.lightmapColor);
            }

            lastBlendValue = blendValue;
            return true;
        }

        internal void SetPresetsToBlend(LightmapPreset presetA, LightmapPreset presetB)
        {
            SetPresetsToBlend(presetA.LightingName, presetB.LightingName);
        }

        internal void SetPresetsToBlend(string presetA, string presetB)
        {
            if (!mappedPresets.TryGetValue(presetA, out this.presetA))
            {
                throw new ArgumentException(nameof(presetA));
            }

            if (!mappedPresets.TryGetValue(presetB, out this.presetB))
            {
                throw new ArgumentException(nameof(presetB));
            }

            isDirty = true;
        }

        internal void SetAllowedIndexes(params int[] indexes)
        {
            allowedIndexes = new HashSet<int>(indexes);
        }

        public void Dispose()
        {
            runtimePreset.Dispose();
            foreach (var shadowMask in shadowMasks)
            {
                Object.Destroy(shadowMask);
            }

            foreach (var lightmapDir in lightmapDirs)
            {
                Object.Destroy(lightmapDir);
            }

            foreach (var lightmapColor in lightmapColors)
            {
                Object.Destroy(lightmapColor);
            }

            shadowMasks.Clear();
            lightmapDirs.Clear();
            lightmapColors.Clear();

            Object.Destroy(blendingMaterial);
        }
    }
}