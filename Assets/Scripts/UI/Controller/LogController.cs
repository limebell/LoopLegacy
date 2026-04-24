using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using TMPro;

namespace LoopLegacy.UI.Controller
{
    public struct LogEntry
    {
        public string message;
        public string color;
        
        public LogEntry(string message, string color)
        {
            this.message = message;
            this.color = color;
        }
    }
    public class LogController : MonoBehaviour
    {
        public static LogController Instance;

        [Header("Log overlay")]
        [Tooltip("에디터/빌드에서 이 오버레이를 사용할지 여부입니다.")]
        [SerializeField] private bool _isEnabled = true;

        [Header("UI references")]
        [SerializeField] private GameObject _element;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _consoleButton;
        [SerializeField] private Button _clearButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_Text _logText;
        [SerializeField] private ScrollRect _logScrollRect;
        private List<LogEntry> _logMessages = new List<LogEntry>();

        void Awake()
        {
#if UNITY_EDITOR
            if (!_isEnabled)
            {
                Destroy(gameObject);
                return;
            }
#else
            if (!Debug.isDebugBuild)
            {
                Destroy(gameObject);
                return;
            }
#endif

            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Debug 클래스 초기화 - 모든 로그 메시지 캐치 시작
                Debug.Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            _openButton.onClick.AddListener(OnOpenButtonClicked);
            _consoleButton.onClick.AddListener(OnConsoleButtonClicked);
            _clearButton.onClick.AddListener(OnClearButtonClicked);
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
            _element.SetActive(false);
        }

        private void OnOpenButtonClicked()
        {
            _element.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_logText.rectTransform);
            Canvas.ForceUpdateCanvases();
            _logScrollRect.verticalNormalizedPosition = 0;
        }

        private void OnConsoleButtonClicked()
        {
            ConsoleController.Instance.Show();
        }

        private void OnClearButtonClicked()
        {
            _logMessages.Clear();
            _logText.text = string.Empty;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_logText.rectTransform);
            _logScrollRect.verticalNormalizedPosition = 0;
        }

        private void OnCloseButtonClicked()
        {
            _element.SetActive(false);
        }

        public void Log(string message)
        {
            AddLogEntry(new LogEntry(message, "#FFFFFF"));
        }

        public void Log(string message, UnityEngine.Object context)
        {
            AddLogEntry(new LogEntry(message + " (" + context + ")", "#FFFFFF"));
        }

        public void LogError(string message)
        {
            AddLogEntry(new LogEntry(message, "#FF0000"));
        }

        public void LogError(string message, UnityEngine.Object context)
        {
            AddLogEntry(new LogEntry(message + " (" + context + ")", "#FF0000"));
        }

        public void LogWarning(string message)
        {
            AddLogEntry(new LogEntry(message, "#FFFF00"));
        }
        
        public void LogWarning(string message, UnityEngine.Object context)
        {
            AddLogEntry(new LogEntry(message + " (" + context + ")", "#FFFF00"));
        }

        private void AddLogEntry(LogEntry entry)
        {
            _logMessages.Add(entry);
            // 새로운 Label 생성
            _logText.text += $"<color={entry.color}>{entry.message}</color>\n";
            LayoutRebuilder.ForceRebuildLayoutImmediate(_logText.rectTransform);
            _logScrollRect.verticalNormalizedPosition = 0;
        }

        public void Assert(Exception exception)
        {
            
        }
    }
}
