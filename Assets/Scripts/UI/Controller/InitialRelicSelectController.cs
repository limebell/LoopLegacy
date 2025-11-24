using LoopLegacy.UI.Component;
using UnityEngine;
using R3;
using LoopLegacy.State;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using LoopLegacy.Loader;
using System;

namespace LoopLegacy.UI.Controller
{
    public class InitialRelicSelectController : MonoBehaviour
    {
        [SerializeField] private Button _leftButton;
        [SerializeField] private Button _rightButton;
        [SerializeField] private TMP_Text _maxGradeText;
        [SerializeField] private ItemContainer[] _itemContainers;
        [SerializeField] private TMP_Text _indexText;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _gradeText;
        [SerializeField] private TMP_Text _descriptionText;

        private List<int> _relicIds = new List<int>();
        private int _currentIndex;

        void Start()
        {
            _itemContainers[1].onClick.AddListener(OnClickRelic);
            _leftButton.onClick.AddListener(OnLeftButtonClicked);
            _rightButton.onClick.AddListener(OnRightButtonClicked);
            PersistentGameState.Instance.HouseState.InitialRelic.Subscribe(OnInitialRelicChanged);
        }

        void OnEnable()
        {
            var maxGrade = (RelicGrade)Mathf.Clamp(
                PersistentGameState.Instance.HouseState.GetUpgradeValue(UpgradeType.InitialRelicSelect),
                0,
                Enum.GetValues(typeof(RelicGrade)).Length - 1);
            _maxGradeText.text = Utils.GetUIString("grade_" + maxGrade.ToString().ToLowerInvariant());
            _maxGradeText.color = Utils.GetRelicGradeColor(maxGrade);
            _relicIds = PersistentGameState.Instance.CodexState.GetAvailableRelics()
                .Where(relic => relic.Grade <= maxGrade)
                .Select(relic => relic.Id).ToList();
            _relicIds.Insert(0, -1);
            _indexText.text = $"{_currentIndex + 1} / {_relicIds.Count}";
            _currentIndex = _relicIds.FindIndex(id => id == PersistentGameState.Instance.HouseState.InitialRelic.Value);
            _leftButton.gameObject.SetActive(_currentIndex > 0);
            _rightButton.gameObject.SetActive(_currentIndex < _relicIds.Count - 1);
        }

        private void OnClickRelic()
        {
            // TODO: 나중에 누르면 리스트 페이지 표시?
        }

        private void OnLeftButtonClicked()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                PersistentGameState.Instance.HouseState.InitialRelic.Value = _relicIds[_currentIndex];
                _indexText.text = $"{_currentIndex + 1} / {_relicIds.Count}";
                _leftButton.gameObject.SetActive(_currentIndex > 0);
                _rightButton.gameObject.SetActive(_currentIndex < _relicIds.Count - 1);
                PersistentGameState.Instance.SaveState();
            }
        }

        private void OnRightButtonClicked()
        {
            if (_currentIndex < _relicIds.Count - 1)
            {
                _currentIndex++;
                PersistentGameState.Instance.HouseState.InitialRelic.Value = _relicIds[_currentIndex];
                _indexText.text = $"{_currentIndex + 1} / {_relicIds.Count}";
                _leftButton.gameObject.SetActive(_currentIndex > 0);
                _rightButton.gameObject.SetActive(_currentIndex < _relicIds.Count - 1);
                PersistentGameState.Instance.SaveState();
            }
        }

        private void OnInitialRelicChanged(int id)
        {
            var leftRelic = _currentIndex > 0 ? PersistentGameState.Instance.CodexState.GetRelic(_relicIds[_currentIndex - 1]) : null;
            var rightRelic = _currentIndex < _relicIds.Count - 1 ? PersistentGameState.Instance.CodexState.GetRelic(_relicIds[_currentIndex + 1]) : null;
            var relic = PersistentGameState.Instance.CodexState.GetRelic(id);
            _itemContainers[0].gameObject.SetActive(_currentIndex > 0);
            _itemContainers[0].SetRelic(leftRelic);
            _itemContainers[2].gameObject.SetActive(_currentIndex < _relicIds.Count - 1);
            _itemContainers[2].SetRelic(rightRelic);
            if (relic == null)
            {
                _itemContainers[1].SetRelic(null);
                _nameText.text = Utils.GetUIString("initial-relic_none");
                _gradeText.text = "";
                _descriptionText.text = Utils.GetUIString("initial-relic_none_description");
                return;
            }
            _itemContainers[1].SetRelic(relic);
            _nameText.text = $"[{relic.Effect.GetName()} Lv. {relic.Level + 1}]";
            _gradeText.text = Utils.GetUIString("grade_" + relic.Grade.ToString().ToLowerInvariant());
            _gradeText.color = Utils.GetRelicGradeColor(relic.Grade);
            _descriptionText.text = relic.Effect.GetDescription();
        }
    }
}