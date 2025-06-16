using System.Collections.Generic;
using CustomConsole.Runtime.Logger;
using UnityEngine;

namespace CustomConsole.Runtime.Console
{
    public class CustomConsoleLogReceiver : MonoBehaviour
    {
        public static List<CustomConsoleManager.LogInfo> logBuffer = new List<CustomConsoleManager.LogInfo>();
        public static bool isInitialized = false;
        public static bool isMainConsoleInitialized = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void EarlyLogCatcherInitialise()
        {
            if (isInitialized) return;
            Application.logMessageReceived += EarlyLogCatcher;
            isInitialized = true;
        }

        public static void EarlyLogCatcher(string logString, string stackTrace, LogType type)
        {
            if (!isMainConsoleInitialized)
            {
                CustomConsoleManager.LogInfo log = new CustomConsoleManager.LogInfo
                {
                    LogTransform = null,
                    LogText = logString,
                    StackTrace = stackTrace,
                    LogType = type
                };
                logBuffer.Add(log);
            }
            else
            {
                if (type == LogType.Log && !logString.Contains(CustomLogger.CUSTOM_LOG_IDENTIFIER))
                {
                    CustomConsoleManager.Instance.AddLog(logString, stackTrace, type, false);
                }
                else
                {
                    CustomConsoleManager.Instance.AddLog(logString, stackTrace, type, true);
                }
            }
        }

        void OnDestroy()
        {
            Application.logMessageReceived -= EarlyLogCatcher;
        }
    }
}
