using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Toolbox.Lighting.Utilities
{
    public static class ObjectUtility
    {
        public static void SafeDestroy(Object target)
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                Object.Destroy(target);
            }
            else
            {
                Object.DestroyImmediate(target);
            }
#else
            Object.Destroy(target);
#endif
        }
    }
}