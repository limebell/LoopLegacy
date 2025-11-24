using LoopLegacy;
using LoopLegacy.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoopLegacy.UI.Controller
{
    public class AddStatController : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statTypeText;
        [SerializeField] private TMP_Text _statDescriptionText;
        [SerializeField] private TMP_Text _statPointsText;
        [SerializeField] private Button _addOneButton;
        [SerializeField] private Button _addHalfButton;
        [SerializeField] private Button _addAllButton;
        [SerializeField] private TMP_InputField _statValueInputField;
        [SerializeField] private Button _applyButton;
        [SerializeField] private Button _cancelButton;

        private StatType _statType;
        private int _initialPoints;
        private int _investedPoints;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _addOneButton.onClick.AddListener(OnAddOneButtonClicked);
            _addHalfButton.onClick.AddListener(OnAddHalfButtonClicked);
            _addAllButton.onClick.AddListener(OnAddAllButtonClicked);
            _statValueInputField.onValueChanged.AddListener(OnStatValueInputFieldChanged);
            _applyButton.onClick.AddListener(OnApplyButtonClicked);
            _cancelButton.onClick.AddListener(OnCancelButtonClicked);
        }

        public void Show(StatType statType, int statPoints)
        {
            gameObject.SetActive(true);
            _statType = statType;
            string statName = Utils.GetStatName(statType);
            _statTypeText.text = statName;
            _statDescriptionText.text = Utils.GetUIString($"stat-description-{statName.ToLower()}");
            _initialPoints = statPoints;
            _investedPoints = 0;
            _statPointsText.text = $"{_initialPoints:n0}";
            _statValueInputField.text = "0";
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnAddOneButtonClicked()
        {
            int remainingPoints = _initialPoints - _investedPoints;
            if (remainingPoints > 0)
            {
                _investedPoints += 1;
                _statPointsText.text = $"{_initialPoints - _investedPoints:n0}";
                _statValueInputField.text = _investedPoints.ToString();
            }
        }

        private void OnAddHalfButtonClicked()
        {
            int remainingPoints = _initialPoints - _investedPoints;
            if (remainingPoints > 0)
            {
                _investedPoints += remainingPoints / 2;
                _statPointsText.text = $"{_initialPoints - _investedPoints:n0}";
                _statValueInputField.text = _investedPoints.ToString();
            }
        }

        private void OnAddAllButtonClicked()
        {
            int remainingPoints = _initialPoints - _investedPoints;
            if (remainingPoints > 0)
            {
                _investedPoints = _initialPoints;
                _statPointsText.text = $"{_initialPoints - _investedPoints:n0}";
                _statValueInputField.text = _investedPoints.ToString();
            }
        }

        private void OnStatValueInputFieldChanged(string value)
        {
            if (int.TryParse(value, out int points))
            {
                if (points > _initialPoints)
                {
                    _investedPoints = _initialPoints;
                }
                else if (points < 0)
                {
                    _investedPoints = 0;
                }
                else
                {
                    _investedPoints = points;
                }
                _statValueInputField.text = _investedPoints.ToString();
                _statPointsText.text = $"{_initialPoints - _investedPoints:n0}";
            }
            else
            {
                _investedPoints = 0;
                _statPointsText.text = $"{_initialPoints:n0}";
                _statValueInputField.text = "0";
            }
        }

        private void OnApplyButtonClicked()
        {
            GameManager.Instance.GameState.PlayerStats.AddStat(_statType, _investedPoints);
            GameManager.Instance.Save();
            Hide();
        }
        
        private void OnCancelButtonClicked()
        {
            Hide();
        }
    }
}
