using System;
using UnityEngine;

namespace CustomConsole.Runtime.Logger
{
    public static class CustomLogger
    {
        public const string CUSTOM_LOG_IDENTIFIER = "[CustomLogger]";

        public enum LogType
        {
            Normal,
            Important,
            Highlight,
            CCError
        }

        public static event Action<LogType, string> OnCustomLogSending;

        private static Color _loggerIdentifierColor = new Color(0.7f, 0.5f, 0.9f);
        private static Color _importantLogColor = Color.cyan;
        private static Color _highlightLogColor = Color.green;
        private static Color _ccErrorLogColor = new Color(1f, 0.35f, 0.1f);

        public static Color ColorLoggerIdentifier
        {
            get => _loggerIdentifierColor;
            private set { }
        }

        public static Color ColorImportantLog
        {
            get => _importantLogColor;
            private set { }
        }

        public static Color ColorHighlightLog
        {
            get => _highlightLogColor;
            private set { }
        }

        public static Color ColorCCErrorLog
        {
            get => _ccErrorLogColor;
            private set { }
        }

        public static void Log(string log)
        {
#if UNITY_2019_1_OR_NEWER
            string identifierColorStr = ColorUtility.ToHtmlStringRGB(ColorLoggerIdentifier);
            Debug.Log($"<color=#{identifierColorStr}>{CUSTOM_LOG_IDENTIFIER}</color> {log}");
            OnCustomLogSending?.Invoke(LogType.Normal, log);
#else
                    Debug.Log($"{CUSTOM_LOG_IDENTIFIER} {log}");
                    OnCustomLogSending?.Invoke(LogType.Normal, log);
#endif
        }

        public static void ImportantLog(string log)
        {
#if UNITY_2019_1_OR_NEWER
            string identifierColorStr = ColorUtility.ToHtmlStringRGB(ColorLoggerIdentifier);
            string importantColorStr = ColorUtility.ToHtmlStringRGB(ColorImportantLog);
            Debug.Log(
                $"<color=#{identifierColorStr}>{CUSTOM_LOG_IDENTIFIER}</color> <color=#{importantColorStr}>[{CustomLogger.LogType.Important}]</color> {log}");
            OnCustomLogSending?.Invoke(LogType.Important, log);
#else
            Debug.Log($"{CUSTOM_LOG_IDENTIFIER} [{CustomLogger.LogType.Important}] {log}");
            OnCustomLogSending?.Invoke(LogType.Important, log);
#endif
        }

        public static void HighlightLog(string log)
        {
#if UNITY_2019_1_OR_NEWER
            string identifierColorStr = ColorUtility.ToHtmlStringRGB(ColorLoggerIdentifier);
            string highlightColorStr = ColorUtility.ToHtmlStringRGB(ColorHighlightLog);
            Debug.Log(
                $"<color=#{identifierColorStr}>{CUSTOM_LOG_IDENTIFIER}</color> <color=#{highlightColorStr}>[{CustomLogger.LogType.Highlight}]</color> {log}");
            OnCustomLogSending?.Invoke(LogType.Highlight, log);
#else
                Debug.Log($"{CUSTOM_LOG_IDENTIFIER} [{CustomLogger.LogType.Highlight}] {log}");
                OnCustomLogSending?.Invoke(LogType.Highlight, log);
#endif
        }

        public static void CCErrorLog(string log)
        {
#if UNITY_2019_1_OR_NEWER
            string identifierColorStr = ColorUtility.ToHtmlStringRGB(ColorLoggerIdentifier);
            string ccErrorColor = ColorUtility.ToHtmlStringRGB(ColorCCErrorLog);
            Debug.Log(
                $"<color=#{identifierColorStr}>{CUSTOM_LOG_IDENTIFIER}</color> <color=#{ccErrorColor}>[{CustomLogger.LogType.CCError}] {log}</color>");
            OnCustomLogSending?.Invoke(LogType.CCError, log);
#else
                Debug.Log($"{CUSTOM_LOG_IDENTIFIER} [{CustomLogger.LogType.CCError}] {log}");
                OnCustomLogSending?.Invoke(LogType.CCError, log);
#endif
        }
    }
}