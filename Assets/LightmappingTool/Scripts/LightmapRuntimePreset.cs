﻿using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Toolbox.Lighting
{
    /// <summary>
    /// Creates runtime lightmap preset based on given mockup.
    /// </summary>
    internal class LightmapRuntimePreset : IDisposable
    {
        internal LightmapPreset TargetPreset { get; private set; }


        internal LightmapRuntimePreset(LightmapPreset mockup)
        {
            TargetPreset = ScriptableObject.CreateInstance<LightmapPreset>();
            var mockupTexturesSets = mockup.TexturesSets;
            var setsCount = mockupTexturesSets.Length;
            var targetTexturesSets = new LightmapTexturesSet[setsCount];

            for (var i = 0; i < setsCount; i++)
            {
                var shadowMask = mockupTexturesSets[i].shadowMask;
                var lightmapDir = mockupTexturesSets[i].lightmapDir;
                var lightmapColor = mockupTexturesSets[i].lightmapColor;

                var texturesSet = new LightmapTexturesSet()
                {
                    shadowMask = CreateRuntimeTexture(shadowMask),
                    lightmapDir = CreateRuntimeTexture(lightmapDir),
                    lightmapColor = CreateRuntimeTexture(lightmapColor)
                };
                targetTexturesSets[i] = texturesSet;
            }

            TargetPreset.TexturesSets = targetTexturesSets;
        }


        private Texture2D CreateRuntimeTexture(Texture2D texture)
        {
            if (!texture)
            {
                return null;
            }

            var newTexture = new Texture2D(texture.width, texture.height,
                UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat,
                texture.mipmapCount,
                UnityEngine.Experimental.Rendering.TextureCreationFlags.MipChain);
            return newTexture;
        }


        public void Dispose()
        {
            Object.Destroy(TargetPreset);
        }
    }
}