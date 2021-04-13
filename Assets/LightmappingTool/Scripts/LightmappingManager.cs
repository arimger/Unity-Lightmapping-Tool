using UnityEngine;

namespace Toolbox.Lighting
{
    public class LightmappingManager : MonoBehaviour
    {
        public enum Mode
        {
            Changer,
            Blender,
        }

        [SerializeField]
        private Mode currentMode = Mode.Blender;

        //TODO: use to is to cache reflection probes
        [SerializeField]
        private bool autoSearchRefs;
        private bool isInitialized;

        [Space]

        [SerializeField]
        private LightmapPreset[] presets;

        private LightmapTransitionPreset transitionPreset;

        [SerializeField]
        private ReflectionProbe[] reflectionProbes;

        [SerializeField, Range(0.0f, 1.0f)]
        private float blendValue;


        private bool ShouldBeUpdated
        {
            get => currentMode == Mode.Blender && isInitialized;
        }


        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (!ShouldBeUpdated)
            {
                return;
            }

            transitionPreset.Update(blendValue);
        }

        private void OnDestroy()
        {
            transitionPreset.Dispose();
        }

        private void Initialize()
        {
            transitionPreset = new LightmapTransitionPreset(presets);
            //transitionPreset.SetAllowedIndexes(0);
            transitionPreset.SetPresetsToBlend(presets[0], presets[1]);
            transitionPreset.Update(0.0f);
            LightmapSettings.lightmaps = transitionPreset.Lightmaps;
            isInitialized = true;
        }
    }
}