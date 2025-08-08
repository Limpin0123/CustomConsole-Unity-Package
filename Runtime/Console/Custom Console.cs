using System.Collections;
using System.Collections.Generic;
using CustomConsole.Runtime.Logger;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CustomConsole.Runtime.Console
{
    [RequireComponent(typeof(CustomConsoleLogReceiver), typeof(CustomConsolePathExtractor))]
    public class CustomConsoleManager : MonoBehaviour
    {
        public static CustomConsoleManager Instance { get; private set; }

        [Header("References")] [SerializeField]
        private Transform contentTransform;

        [SerializeField] private Transform releaseBuildWarning;
        [SerializeField] private Button resetButton;
        [SerializeField] private TextMeshProUGUI logAmountText;
        [SerializeField] private ScrollRect mainAreaScrollRect;

        [Header("Prefabs")] [SerializeField] private Transform clickableLogPrefab;

        [FormerlySerializedAs("maxLogAmount")] [Header("Parameters")] [SerializeField]
        private int maxLogAmount = 25;

        [SerializeField] private bool copyFullPathInBuild = false;

        private CustomConsolePathExtractor customConsolePathExtractor;
        private int _currentID;
        private Dictionary<int, LogInfo> _logs;

        public class LogInfo
        {
            public EntryUpdater EntryUpdater;
            public Transform LogTransform;
            public string LogText;
            public string StackTrace;
            public LogType LogType;
        }

        void Awake()
        {
            if (Instance != null) Destroy(this.gameObject);
            Instance = this;
            CustomConsoleLogReceiver.isMainConsoleInitialized = true;
            customConsolePathExtractor = GetComponent<CustomConsolePathExtractor>();

            resetButton.onClick.AddListener(ResetLogConsole);

            if (!Debug.isDebugBuild) releaseBuildWarning.gameObject.SetActive(true);
            else releaseBuildWarning.gameObject.SetActive(false);

            _currentID = 0;
            _logs = new Dictionary<int, LogInfo>();

            for (int i = 0; i < maxLogAmount; i++)
            {
                CreateLog();
            }

            foreach (LogInfo logInfo in CustomConsoleLogReceiver.logBuffer)
            {
                if (logInfo.LogType == LogType.Log && !logInfo.LogText.Contains(CustomLogger.CUSTOM_LOG_IDENTIFIER))
                {
                    AddLog(logInfo.LogText, logInfo.StackTrace, logInfo.LogType, false);
                }
                else
                {
                    AddLog(logInfo.LogText, logInfo.StackTrace, logInfo.LogType, true);
                }
            }
        }

        #region Log visual

        void CreateLog()
        {
            Transform newLog = Instantiate(clickableLogPrefab, contentTransform);
            EntryUpdater entry = newLog.GetComponent<EntryUpdater>();
            entry.HideEntry();
            _logs.Add(_logs.Count,
                new LogInfo()
                {
                    EntryUpdater = entry, LogTransform = newLog, LogText = "logString", StackTrace = "stackTrace",
                    LogType = LogType.Log
                });
        }

        public void AddLog(string logString, string stackTrace, LogType type, bool isClickableLog)
        {
            if (_currentID >= maxLogAmount) //if there's to many log, delete the oldest one
            {
                FreeLastIndex();
                _currentID = maxLogAmount - 1;
            }

            LogInfo newLogInfo = new LogInfo
            {
                EntryUpdater = _logs[_currentID].EntryUpdater,
                LogTransform = _logs[_currentID].LogTransform,
                LogText = logString,
                StackTrace = stackTrace,
                LogType = type
            };
            UpdateLog(logString, stackTrace, type, _currentID);
            _logs[_currentID].EntryUpdater.ShowEntry();

            _currentID++;

            //Update the log
            UpdateLogCounter();
            StartCoroutine(UpdateScrollArea());
        }

        IEnumerator UpdateScrollArea()
        {
            yield return null;
            mainAreaScrollRect.verticalNormalizedPosition = 0f;
        }

        void GetAndOpenLogPath(string stackTrace)
        {
            if (customConsolePathExtractor.TryToGetPathAndLine(stackTrace, out string path, out int line))
            {
                customConsolePathExtractor.copyFullPath = copyFullPathInBuild;
                customConsolePathExtractor.OpenFile(path, line);
            }
            else
            {
                if (path != "") Debug.Log($"line wasn't found \n {stackTrace}");
                else Debug.Log("Path wasn't found");
            }
        }

        private void FreeLastIndex()
        {
            for (int i = 0; i < maxLogAmount - 1; i++)
            {
                UpdateLog(_logs[i + 1].LogText, _logs[i + 1].StackTrace, _logs[i + 1].LogType, i);
            }
        }

        void UpdateLog(string logString, string stackTrace, LogType type, int logID)
        {
            _logs[logID].LogText = logString;
            _logs[logID].StackTrace = stackTrace;
            _logs[logID].LogType = type;

            Color color = (type == LogType.Log) ? Color.white :
                (type == LogType.Warning) ? new Color(1, 0.75f, 0.03f) : new Color(1, 0.43f, 0.25f);
            _logs[logID].EntryUpdater.UpdateEntryText(logString, color);
            //Action won't be added if is not clickable
            bool isClickableLog = type != LogType.Log || logString.Contains("[CustomLogger]");
            _logs[logID].EntryUpdater.UpdateClickableArea(() => GetAndOpenLogPath(stackTrace), isClickableLog);
        }

        private void UpdateLogCounter()
        {
            logAmountText.text = _currentID.ToString();
        }

        #endregion

        private void ResetLogConsole()
        {
            for (int i = maxLogAmount - 1; i >= 0; i--)
            {
                _logs[i].EntryUpdater.HideEntry();
            }

            _currentID = 0;
            UpdateLogCounter();
        }
    }
}
