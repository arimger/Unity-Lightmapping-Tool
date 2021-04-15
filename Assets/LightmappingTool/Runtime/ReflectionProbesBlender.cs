using UnityEngine;

namespace Toolbox.Lighting
{
    internal class ReflectionProbesBlender
    {
        private readonly CachedReflectionProbe[] targetProbes;

        private float lastBlendValue;


        internal ReflectionProbesBlender(CachedReflectionProbe[] targetProbes)
        {
            this.targetProbes = targetProbes;
        }


        internal bool Update(float blendValue)
        {
            if (Mathf.Approximately(lastBlendValue, blendValue))
            {
                return false;
            }

            lastBlendValue = blendValue;
            return true;
        }
    }
}