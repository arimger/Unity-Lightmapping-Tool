using System;
using UnityEngine;

namespace Toolbox.Lighting
{
    [DefaultExecutionOrder(-1000)]
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

        [SerializeField]
        private bool isInitialized;
        [SerializeField]
        private bool initOnAwake = true;

        [Space]

        [SerializeField]
        private LightmapPreset[] presets;

        [SerializeField]
        private CachedReflectionProbe[] probes;

        [SerializeField]
        private LightmapTransitionPreset transitionPreset;

        [SerializeField, Range(0.0f, 1.0f)]
        private float blendValue;


        public Mode CurrentMode
        {
            get => currentMode;
        }

        public bool IsInitialized
        {
            get => isInitialized;
        }

        /// <summary>
        /// Presets used to initialize blending processes.
        /// </summary>
        public LightmapPreset[] Presets
        {
            get => presets;
        }

        public int? PresetsToBlendCount
        {
            get => transitionPreset?.PresetsToBlendCount;
        }

        /// <summary>
        /// Adjust this value to change current blending state.
        /// </summary>
        public float BlendValue
        {
            get => blendValue;
            set => blendValue = value;
        }

        private bool ShouldBeUpdated
        {
            get => currentMode == Mode.Blending && isInitialized;
        }


        private void Awake()
        {
            if (initOnAwake)
            {
                Initialize(currentMode);
            }
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

        private void SetLightmaps(LightmapPreset preset)
        {
            SetLightmaps(preset.Lightmaps);
        }

        private void SetLightmaps(LightmapData[] lightmaps)
        {
            LightmapSettings.lightmaps = lightmaps;
        }
#if UNITY_EDITOR
        private void LogInvalidMode(string operationName)
        {
            Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, this,
                $"Cannot perform operation ({operationName}) in current mode ({currentMode}).");
        }
#endif

        public void Initialize(Mode mode)
        {
            switch (mode)
            {
                case Mode.Blending:
                    ChangeModeToBlending(true, presets);
                    break;
                case Mode.Switcher:
                    ChangeModeToSwitcher(presets[0]);
                    break;
            }

            isInitialized = true;
        }

        public void ChangeModeToSwitcher(LightmapPreset presetToSwitch = null)
        {
            currentMode = Mode.Switcher;
            if (presetToSwitch != null)
            {
                SetLightmaps(presetToSwitch);
            }
        }

        public void ChangeModeToBlending(bool reset, params LightmapPreset[] presetsToBlend)
        {
            currentMode = Mode.Blending;
            if (transitionPreset == null)
            {
                transitionPreset = new LightmapTransitionPreset(presets);
            }
            else if (reset)
            {
                transitionPreset = new LightmapTransitionPreset(presets);
            }

            transitionPreset.SetPresetsToBlend(presetsToBlend);
            //TODO:
            transitionPreset.Update(blendValue - 0.01f);
            transitionPreset.Update(blendValue);
            SetLightmaps(transitionPreset.Lightmaps);
        }

        public void SetPresetsToBlend(params LightmapPreset[] presetsToBlend)
        {
            if (currentMode == Mode.Switcher)
            {
                LogInvalidMode(nameof(SetPresetsToBlend));
                return;
            }

            transitionPreset.SetPresetsToBlend(presetsToBlend);
        }

        public void SetAllowedIndexes(bool[] allowedIndexes)
        {
            if (currentMode == Mode.Switcher)
            {
                LogInvalidMode(nameof(SetAllowedIndexes));
                return;
            }

            transitionPreset.SetAllowedIndexes(allowedIndexes);
        }

        public bool IsPresetBlended(LightmapPreset preset, out int order)
        {
            order = -1;
            if (currentMode == Mode.Switcher)
            {
                return false;
            }

            return transitionPreset.IsPresetBlended(preset, out order);
        }

        public void SearchForReflectionProbes()
        {
            probes = FindObjectsOfType<CachedReflectionProbe>();
        }

        public void SwitchLightmap(LightmapPreset preset)
        {
            if (currentMode == Mode.Blending)
            {
                LogInvalidMode(nameof(SwitchLightmap));
                return;
            }

            if (preset == null)
            {
                throw new ArgumentNullException(nameof(preset));
            }

            SetLightmaps(preset.Lightmaps);
        }
    }
}