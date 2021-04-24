using UnityEngine;

namespace Toolbox.Lighting
{
    internal static class InternalLogger
    {
        private const string tag = "Lightmapping Tool";
        private const string format = "[{0}] {1}";


        internal static void Log(LogType logType, string message)
        {
#if UNITY_2020_1_OR_NEWER
            Debug.LogFormat(logType, LogOption.NoStacktrace, null, format, tag, message);
#else
            Debug.unityLogger.LogFormat(logType, format, tag, message);
#endif
        }
    }
}