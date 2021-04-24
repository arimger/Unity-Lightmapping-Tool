using System;
using UnityEngine;

namespace Toolbox.Lighting
{
    [Obsolete]
    [AddComponentMenu("Toolbox/Lighting/Cached Reflection Probe")]
    [RequireComponent(typeof(ReflectionProbe))]
    public class CachedReflectionProbe : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private ReflectionProbe reflectionProbe;


        public ReflectionProbe ReflectionProbe
        {
            get
            {
                if (reflectionProbe == null)
                {
                    reflectionProbe = GetComponent<ReflectionProbe>();
                }

                return reflectionProbe;
            }
        }
    }
}