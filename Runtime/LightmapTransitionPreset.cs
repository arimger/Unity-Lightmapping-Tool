using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Toolbox.Lighting
{
    using Toolbox.Lighting.Utilities;

    /// <summary>
    /// Class responsible for blending multiple lightmaps into one.
    /// </summary>
    [Serializable]
    public class LightmapTransitionPreset : IDisposable
    {
        public const int minimalPresetsToBlend = 2;

        ///----------------------------------------///
        ///         Transition collections         ///
        ///----------------------------------------///

        private readonly List<RenderTexture> shadowMasks = new List<RenderTexture>();
        private readonly List<RenderTexture> lightmapDirs = new List<RenderTexture>();
        private readonly List<RenderTexture> lightmapColors = new List<RenderTexture>();
        private readonly List<RenderTexture> reflectionProbes = new List<RenderTexture>();

        /// <summary>
        /// Material used to blend standard 2D textures of the lightmap.
        /// </summary>
        private readonly Material blendingMaterial;

        private readonly bool? isSafe;

        private LightmapRuntimePreset runtimePreset;

        [SerializeField, HideInInspector]
        private float lastBlendValue;

        [SerializeField]
#if UNITY_2020_1_OR_NEWER
        [NonReorderable]
#endif
        private LightmapPreset[] blendedPresets;

        [SerializeField]
#if UNITY_2020_1_OR_NEWER
        [NonReorderable]
#endif
        private bool[] allowedIndexes;

        /// <summary>
        /// Used to speed-up checks.
        /// </summary>
        public Dictionary<LightmapPreset, int> mappedBlendedPresets;


        internal int PresetsToBlendCount
        {
            get; private set;
        }

        internal LightmapPreset[] BlendedPresets
        {
            get => blendedPresets;
        }

        public bool[] AllowedIndexes
        {
            get => allowedIndexes;
        }

        /// <summary>
        /// Indicates if preset is properly initialized (by constructor) and ready to use.
        /// </summary>
        internal bool IsSafe
        {
            get => isSafe.HasValue && isSafe.Value;
        }

        internal bool IsDirty
        {
            get; private set;
        }

        /// <summary>
        /// Indicates if preset is properly initialized.
        /// </summary>
        internal bool IsReady
        {
            get => runtimePreset != null && runtimePreset.TargetPreset;
        }

        internal bool HasProbes
        {
            get; private set;
        }

        internal bool UseProbes
        {
            get; set;
        }

        /// <summary>
        /// Lightmaps created for the runtime processing.
        /// </summary>
        internal LightmapData[] Lightmaps
        {
            get => runtimePreset.TargetPreset.Lightmaps;
        }

        internal LightProbes LightProbes
        {
            get => runtimePreset.TargetPreset.LightProbes;
        }

        /// <summary>
        /// Fired each time runtime preset is regenerated and ready to use.
        /// </summary>
        internal event Action OnReady;


        internal LightmapTransitionPreset(Material blendingMaterial)
        {
            this.blendingMaterial = blendingMaterial;

            shadowMasks = new List<RenderTexture>();
            lightmapDirs = new List<RenderTexture>();
            lightmapColors = new List<RenderTexture>();
            isSafe = true;
        }


        private void PrepareRuntimePreset()
        {
            //TODO: do it in time
            var mockupPreset = blendedPresets[0];
            if (mockupPreset == null)
            {
                return;
            }

            runtimePreset = new LightmapRuntimePreset(mockupPreset);
            var texturesSets = runtimePreset.TargetPreset.TexturesSets;
            allowedIndexes = new bool[texturesSets.Length];

            //prepre transition textures sets related to color/directional/shadowmask maps
            for (var i = 0; i < texturesSets.Length; i++)
            {
                var texturesSet = texturesSets[i];
                shadowMasks.Add(CreateTransitionTexture(texturesSet.shadowMask));
                lightmapDirs.Add(CreateTransitionTexture(texturesSet.lightmapDir));
                lightmapColors.Add(CreateTransitionTexture(texturesSet.lightmapColor));
                allowedIndexes[i] = true;
            }

            OnReady?.Invoke();
        }

        private void PrepareInSceneProbes()
        {                
            //TODO: prepare transition textures for available reflection probes
            if (UseProbes)
            {
                //for (var i = 0; i < cachedSceneProbes.Length; i++)
                //{
                //    var reflectionProbe = cachedSceneProbes[i].ReflectionProbe;
                //    var bakedTexture = reflectionProbe.bakedTexture;
                //    if (bakedTexture == null)
                //    {
                //        continue;
                //    }

                //    var transitionTexture = CreateTransitionTexture(bakedTexture as Cubemap);
                //    reflectionProbes.Add(transitionTexture);
                //    reflectionProbe.bakedTexture = transitionTexture;
                //}
            }

            HasProbes = reflectionProbes.Count > 0;
        }

        private IEnumerator PrepareRuntimePresetInTime()
        {
            //TODO:
            yield return null;
        }

        private RenderTexture CreateTransitionTexture(Texture2D texture)
        {
            if (texture == null)
            {
                return null;
            }

            return new RenderTexture(texture.width, texture.height, 0)
            {
                useMipMap = true,
                graphicsFormat = texture.graphicsFormat
            };
        }

        private RenderTexture CreateTransitionTexture(Cubemap cubemap)
        {
            if (cubemap == null)
            {
                return null;
            }

            return new RenderTexture(cubemap.width, cubemap.height, cubemap.mipmapCount)
            {
                dimension = TextureDimension.Cube,
                useMipMap = true,
                graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat
            };
        }

        private void BlendRuntimeLightmaps(float blendValue, int indexA, int indexB)
        {
            //set current blending value (between 0 and 1)
            blendingMaterial.SetFloat("_Blend", blendValue);

            var targetLightmaps = Lightmaps;
            var lightmapsCount = targetLightmaps.Length;
            var lightmapsA = blendedPresets[indexA].Lightmaps;
            var lightmapsB = blendedPresets[indexB].Lightmaps;

            for (var i = 0; i < lightmapsCount; i++)
            {
                if (!allowedIndexes[i])
                {
                    continue;
                }

                const string blendTextureName = "_BlendTex";

                var target = targetLightmaps[i];
                var lightmapA = lightmapsA[i];
                var lightmapB = lightmapsB[i];

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
        }

        private void BlendReflectionProbes(float blendValue, int indexA, int indexB)
        {
            if (!HasProbes)
            {
                return;
            }

            var probesA = blendedPresets[indexA].ReflectionProbes;
            var probesB = blendedPresets[indexB].ReflectionProbes;
            for (var i = 0; i < reflectionProbes.Count; i++)
            {
                var probeA = probesA[i];
                var probeB = probesB[i];
                ReflectionProbe.BlendCubemap(probeA, probeB, blendValue, reflectionProbes[i]);
            }
        }


        internal bool Update(float blendValue)
        {
            if (!IsReady || (Mathf.Approximately(lastBlendValue, blendValue) && !IsDirty))
            {
                return false;
            }

            //calculate currently blended presets using blendValue and overall count
            var step = 1.0f / (PresetsToBlendCount - 1);
            var indexA = Mathf.Min((int)(blendValue / step), PresetsToBlendCount - 2);
            var indexB = indexA + 1;
            var realBlendValue = (blendValue - indexA * step) / step;

            //try to blend lightmaps and probes
            BlendRuntimeLightmaps(realBlendValue, indexA, indexB);
            BlendReflectionProbes(realBlendValue, indexA, indexB);

            lastBlendValue = blendValue;
            IsDirty = false;
            return true;
        }

        internal void SetPresetsToBlend(params LightmapPreset[] presetsToBlend)
        {
            if (presetsToBlend == null)
            {
                throw new ArgumentNullException(nameof(presetsToBlend));
            }

            if (presetsToBlend.Length < minimalPresetsToBlend)
            {
                throw new ArgumentException("Not enough presets to blend.", nameof(presetsToBlend));
            }

            //TODO: presets validation?
            blendedPresets = presetsToBlend;

            if (!IsReady)
            {
                PrepareRuntimePreset();
            }

            PresetsToBlendCount = presetsToBlend.Length;
            mappedBlendedPresets = new Dictionary<LightmapPreset, int>(PresetsToBlendCount);
            for (var i = 0; i < PresetsToBlendCount; i++)
            {
                var preset = presetsToBlend[i];
                if (mappedBlendedPresets.ContainsKey(preset))
                {
                    continue;
                }
                else
                {
                    mappedBlendedPresets.Add(preset, i);
                }
            }

            IsDirty = true;
        }

        internal void SetAllowedIndexes(bool[] allowedIndexes)
        {
            if (allowedIndexes == null)
            {
                throw new ArgumentNullException(nameof(allowedIndexes));
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

        internal bool GetIndexAllowed(int index)
        {
            return allowedIndexes[index];
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
                ObjectUtility.SafeDestroy(shadowMask);
            }

            foreach (var lightmapDir in lightmapDirs)
            {
                ObjectUtility.SafeDestroy(lightmapDir);
            }

            foreach (var lightmapColor in lightmapColors)
            {
                ObjectUtility.SafeDestroy(lightmapColor);
            }

            foreach (var reflectionProbe in reflectionProbes)
            {
                ObjectUtility.SafeDestroy(reflectionProbe);
            }

            shadowMasks.Clear();
            lightmapDirs.Clear();
            lightmapColors.Clear();

            ObjectUtility.SafeDestroy(blendingMaterial);
        }
    }
}