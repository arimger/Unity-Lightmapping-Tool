using UnityEngine;

namespace Toolbox.Lighting
{
    [CreateAssetMenu(fileName = "Lightmap Preset", menuName = "Toolbox/Lighting/Lightmap Preset")]
    public class LightmapPreset : ScriptableObject
    {
        [SerializeField]
        private string presetName;

        [Space]

        [SerializeField]
        private LightmapTexturesSet[] texturesSets;

        [SerializeField]
        private Texture[] reflectionProbes;

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

        public Texture[] ReflectionProbes 
        {
            get => reflectionProbes; 
            set => reflectionProbes = value;
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
    }
}