using UnityEngine;

namespace Toolbox.Lighting
{
    [AddComponentMenu("Toolbox/Lighting/Lightmapping Manager")]
    public class LightmappingManager : MonoBehaviour
    {
        public enum Mode
        {
            Switcher,
            Blending,
        }

        [SerializeField]
        private Mode currentMode = Mode.Blending;

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
            get => currentMode == Mode.Blending && isInitialized;
        }

        public Mode CurrentMode
        {
            get => currentMode;
        }


        private void Start()
        {
            Initialize(currentMode);
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
            transitionPreset?.Dispose();
        }

        private void Initialize(Mode mode)
        {
            switch (mode)
            {
                case Mode.Blending:
                    transitionPreset = new LightmapTransitionPreset(presets);
                    transitionPreset.SetPresetsToBlend(presets);
                    transitionPreset.Update(0.0f);
                    LightmapSettings.lightmaps = transitionPreset.Lightmaps;
                    break;
                case Mode.Switcher:
                    LightmapSettings.lightmaps = presets[0].Lightmaps;
                    break;
            }

            isInitialized = true;
        }


        public void ChangeMode(Mode mode)
        { 
        }
    }
}