using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LoopLegacy.UI.Component
{
    public class SaveSlotElement : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private TextMeshProUGUI _slotText;
        [SerializeField] private TextMeshProUGUI _genText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _bpText;
        [SerializeField] private TextMeshProUGUI _timeText;

        public string SlotText
        {
            get => _slotText.text;
            set => _slotText.text = value;
        }
        
        public string GenText
        {
            get => _genText.text;
            set => _genText.text = value;
        }
        
        public string LevelText
        {
            get => _levelText.text;
            set => _levelText.text = value;
        }
        
        public string BpText
        {
            get => _bpText.text;
            set => _bpText.text = value;
        }
        
        public string TimeText
        {
            get => _timeText.text;
            set => _timeText.text = value;
        }

        public UnityEvent OnStartButtonClicked;
        public UnityEvent OnDeleteButtonClicked;

        private void Start()
        {
            _startButton.onClick.AddListener(() => OnStartButtonClicked?.Invoke());
            _deleteButton.onClick.AddListener(() => OnDeleteButtonClicked?.Invoke());
        }

        public void SetDataValid(bool isValid)
        {
            _deleteButton.gameObject.SetActive(isValid);
            _startButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                isValid ?
                    Utils.GetUIString("start") :
                    Utils.GetUIString("new-game");
            if (!isValid)
            {
                _genText.text = Utils.GetUIString("new-hero");
                _levelText.text = Utils.GetUIString("new-hero");
                _bpText.text = "-";
                _timeText.text = "0:00:00";
            }
        }
    }
}