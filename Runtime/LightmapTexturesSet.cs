using System;
using UnityEngine;

namespace Toolbox.Lighting
{
    [Serializable]
    public class LightmapTexturesSet
    {
        public Texture2D shadowMask;
        public Texture2D lightmapDir;
        public Texture2D lightmapColor;
    }
}