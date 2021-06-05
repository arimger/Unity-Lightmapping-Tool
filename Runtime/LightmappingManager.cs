using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;

[assembly: InternalsVisibleTo("LightmappingTool-Editor")]
#endif

namespace Toolbox.Lighting
{
    [DefaultExecutionOrder(-1000), DisallowMultipleComponent, ExecuteAlways]
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
        private bool initOnAwake = true;
#if UNITY_EDITOR
        [SerializeField]
        private bool useEditMode = true;
#endif
        [SerializeField, FormerlySerializedAs("presets")]
        private LightmapPreset[] initialPresets;

        [SerializeField]
#if UNITY_2020_1_OR_NEWER
        [NonReorderable]
#endif
        private CachedReflectionProbe[] cachedProbes;

        [SerializeField]
        private LightmapTransitionPreset blendingPreset;

        [SerializeField, Range(0.0f, 1.0f)]
        private float blendValue;


        public Mode CurrentMode
        {
            get => currentMode;
        }

        /// <summary>
        /// Indicates if no property is blocking <see cref="LightmappingManager"/> from initialization or updates.
        /// </summary>
        public bool IsAbleToWork
        {
            get
            {
#if UNITY_EDITOR
                return useEditMode || EditorApplication.isPlaying;
#else
                return true;
#endif
            }
        }

        public bool IsAbleToBlend
        {
            get => blendingPreset != null ? blendingPreset.IsReady : false;
        }

        /// <summary>
        /// Presets used to initialize blending processes.
        /// </summary>
        public LightmapPreset[] Presets
        {
            get => initialPresets;
        }

        public int PresetsToBlendCount
        {
            get => blendingPreset != null ? blendingPreset.PresetsToBlendCount : 0;
        }

        /// <summary>
        /// Adjust this value to change current blending state.
        /// </summary>
        public float BlendValue
        {
            get => blendValue;
            set => blendValue = value;
        }


        private void Awake()
        {
            if (initOnAwake)
            {
                Initialize(currentMode);
            }
            else
            {
                ChangeMode(currentMode);
            }
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!IsAbleToWork)
            {
                return;
            }
#endif
            if (currentMode != Mode.Blending)
            {
                return;
            }

            //NOTE: during recompilation in the Edit mode we will lose all properties
#if UNITY_EDITOR
            if (blendingPreset.IsSafe)
            {
                blendingPreset.Update(blendValue);
            }
            else
            {
                //reload blending preset with last serialized values
                RestoreBlendingPreset();
            }
#else
            blendingPreset.Update(blendValue);
#endif
        }

        private void OnDestroy()
        {
            blendingPreset?.Dispose();
        }


        private void Initialize(Mode mode)
        {
            switch (mode)
            {
                case Mode.Blending:
                    ChangeModeToBlending(true, initialPresets);
                    break;
                case Mode.Switcher:
                    ChangeModeToSwitcher();
                    break;
            }
        }

        private void SetLightmaps(LightmapPreset preset)
        {
            SetLightmaps(preset.Lightmaps, preset.LightProbes);
        }

        private void SetLightmaps(LightmapData[] lightmaps, LightProbes lightProbes)
        {
            LightmapSettings.lightmaps = lightmaps;
            LightmapSettings.lightProbes = lightProbes;
        }

        private void LogModeError(string operationName)
        {
#if UNITY_EDITOR
            InternalLogger.Log(LogType.Error, $"Cannot perform operation ({operationName}) in current mode ({currentMode}).");
#endif
        }

        private void RestoreBlendingPreset()
        {
            if (blendingPreset == null)
            {
                return;
            }

            var presetsToBlend = blendingPreset.BlendedPresets;
            var allowedIndexes = blendingPreset.AllowedIndexes;
            ChangeModeToBlending();
            SetPresetsToBlend(presetsToBlend);
            SetAllowedIndexes(allowedIndexes);
        }


        public bool ChangeMode(Mode mode)
        {
            switch (mode)
            {
                case Mode.Switcher:
                    return ChangeModeToSwitcher();
                case Mode.Blending:
                    return ChangeModeToBlending();
                default:
                    return false;
            }
        }

        public bool ChangeModeToSwitcher(LightmapPreset presetToSwitch = null)
        {
            currentMode = Mode.Switcher;
            if (presetToSwitch != null)
            {
                SetLightmaps(presetToSwitch);
            }

            return true;
        }

        public bool ChangeModeToBlending(bool reset = false, params LightmapPreset[] presetsToBlend)
        {
            currentMode = Mode.Blending;
            //NOTE: we have to assing runtime preset when it's ready
            if (blendingPreset == null || !blendingPreset.IsSafe || reset)
            {
                blendingPreset = new LightmapTransitionPreset(new Material(Shader.Find("Hidden/Lightmap Blend")));
                blendingPreset.OnReady += () =>
                {
                    SetLightmaps(blendingPreset.Lightmaps, blendingPreset.LightProbes);
                };
            }
            else if (blendingPreset.IsReady)
            {
                SetLightmaps(blendingPreset.Lightmaps, blendingPreset.LightProbes);
            }

            //use initial array to initialize current blending preset
            if (presetsToBlend != null &&
                presetsToBlend.Length >= LightmapTransitionPreset.minimalPresetsToBlend)
            {
                SetPresetsToBlend(presetsToBlend);
            }

            return true;
        }

        public void SetPresetsToBlend(params LightmapPreset[] presetsToBlend)
        {
            if (currentMode == Mode.Switcher)
            {
                LogModeError(nameof(SetPresetsToBlend));
                return;
            }

            blendingPreset.SetPresetsToBlend(presetsToBlend);
            blendingPreset.Update(blendValue - 0.01f);
            blendingPreset.Update(blendValue);
        }

        public void SetAllowedIndexes(bool[] allowedIndexes)
        {
            if (currentMode == Mode.Switcher)
            {
                LogModeError(nameof(SetAllowedIndexes));
                return;
            }

            blendingPreset.SetAllowedIndexes(allowedIndexes);
        }

        public bool IsPresetBlended(LightmapPreset preset)
        {
            return IsPresetBlended(preset, out _);
        }

        public bool IsPresetBlended(LightmapPreset preset, out int order)
        {
            order = -1;
            if (currentMode == Mode.Switcher)
            {
                return false;
            }

            return blendingPreset.IsPresetBlended(preset, out order);
        }

        public void SwitchLightmaps(LightmapPreset preset)
        {
            if (currentMode == Mode.Blending)
            {
                LogModeError(nameof(SwitchLightmaps));
                return;
            }

            if (preset == null)
            {
                throw new ArgumentNullException(nameof(preset));
            }

            SetLightmaps(preset.Lightmaps, preset.LightProbes);
        }

        public void SearchForReflectionProbes()
        {
            SearchForReflectionProbes(false);
        }
        
        public void SearchForReflectionProbes(bool includeInactive)
        {
            cachedProbes = FindObjectsOfType<CachedReflectionProbe>(includeInactive);
        }
    }
}