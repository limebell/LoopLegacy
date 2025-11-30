using System;
using System.Collections.Generic;
using LoopLegacy.Battle;
using LoopLegacy.Manager;
using LoopLegacy.State;
using LoopLegacy.UI.Component;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using R3;
using System.Linq;

namespace LoopLegacy.UI.Controller
{
    public class RelicRewardController : MonoBehaviour
    {
        [SerializeField] private Button _rerollButton;
        [SerializeField] private TMP_Text _rerollCountText;
        [SerializeField] private ItemContainer[] _relicRewardContainers;
        [SerializeField] private GameObject _warningText;
        [SerializeField] private TMP_Text _relicCountText;
        [SerializeField] private ItemContainer[] _ownedRelics;
        [SerializeField] TMP_Text _nameText;
        [SerializeField] TMP_Text _gradeText;
        [SerializeField] TMP_Text _descriptionText;
        [SerializeField] Button _acquireDiscardButton;
        [SerializeField] Button _skipRewardButton;

        private int _weight;
        const string _outlineColor = "#FFF0BD";
        const string _outlineColorSelected = "#FF4A32";

        private Relic[] _relicRewards;
        private Relic[] _alreadyRolledRelics;
        private (ItemContainer conatiner, Relic relic, bool isReward) _selectedRelic;

        void Start()
        {
            _rerollButton.onClick.AddListener(OnRerollButtonClicked);
            _acquireDiscardButton.onClick.AddListener(OnAcquireDiscardButtonClicked);
            _skipRewardButton.onClick.AddListener(OnSkipRewardButtonClicked);
            SubscribeToRerollCount();
        }

        private void SubscribeToRerollCount()
        {
            var d = Disposable.CreateBuilder();
            GameManager.Instance.GameState.RelicRewardRerollCount.Subscribe(
                count => {
                    _rerollButton.gameObject.SetActive(count > 0);
                    _rerollCountText.gameObject.SetActive(count > 0);
                    _rerollCountText.text = $"{Utils.GetUIString("reroll-count")}: {count}";
                }).AddTo(ref d);
            d.RegisterTo(this.destroyCancellationToken);
        }

        public void Show(int weight, IEnumerable<Relic> relics, IEnumerable<Relic> alreadyRolledRelics)
        {
            if (!relics.Any())
            {
                Debug.Log("RelicRewardController: No any rewarded relics");
                return;
            }

            if (relics.Count() > _relicRewardContainers.Length)
            {
                Debug.LogError("RelicRewardController: relics count must be between 1 and " + _relicRewardContainers.Length + ", but got " + relics.Count());
                return;
            }

            _weight = weight;
            _relicRewards = relics.ToArray();
            _alreadyRolledRelics = alreadyRolledRelics.ToArray();

            // Display relic rewards
            for (int i = 0; i < _relicRewardContainers.Length; i++)
            {
                if (i < _relicRewards.Length)
                {
                    int index = i;
                    _relicRewardContainers[i].gameObject.SetActive(true);
                    _relicRewardContainers[i].onClick.AddListener(() => SelectRelic(_relicRewardContainers[index], _relicRewards[index], true));
                    _relicRewardContainers[i].SetRelic(_relicRewards[index]);
                    _relicRewardContainers[i].OutlineColor = Utils.HexToColor(_outlineColor);
                }
                else
                {
                    _relicRewardContainers[i].gameObject.SetActive(false);
                    _relicRewardContainers[i].onClick.RemoveAllListeners();
                }
            }

            // Display owned relics
            RefreshOwnedRelics();
            SelectRelic(null, null, true);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void RefreshOwnedRelics()
        {
            var ownedRelics = GameManager.Instance.GameState.OwnedRelics.Value;
            
            if (ownedRelics.Count < PersistentGameState.Instance.GetMaxRelicCount())
            {
                _warningText.SetActive(false);
            }
            else
            {
                // 최대 갯수이면 획득 불가
                _warningText.SetActive(true);
            }

            _relicCountText.text = $"{ownedRelics.Count}/{PersistentGameState.Instance.GetMaxRelicCount()}";
            for (int i = 0; i < _ownedRelics.Length; i++)
            {
                if (i < ownedRelics.Count)
                {
                    int index = i;
                    _ownedRelics[i].gameObject.SetActive(true);
                    _ownedRelics[i].onClick.AddListener(() => SelectRelic(_ownedRelics[index], ownedRelics[index], false));
                    _ownedRelics[i].SetRelic(ownedRelics[index]);
                    _ownedRelics[i].OutlineColor = Utils.HexToColor(_outlineColor);
                }
                else
                {
                    _ownedRelics[i].gameObject.SetActive(false);
                    _ownedRelics[i].onClick.RemoveAllListeners();
                }
            }
        }

        private void SelectRelic(ItemContainer container, Relic relic, bool isReward)
        {
            if (_selectedRelic.conatiner != null)
            {
                _selectedRelic.conatiner.OutlineColor = Utils.HexToColor(_outlineColor);
            }

            _acquireDiscardButton.GetComponentInChildren<TextMeshProUGUI>().text =
                isReward ? Utils.GetUIString("acquire") : Utils.GetUIString("discard");

            if (relic == null)
            {
                _nameText.text = "";
                _gradeText.text = "";
                _descriptionText.text = "";
                _selectedRelic = (null, null, true);
                return;
            }

            container.OutlineColor = Utils.HexToColor(_outlineColorSelected);
            string name = $"{relic.Effect.GetName()} Lv. {relic.Level + 1}";
            string grade = Utils.GetUIString($"grade_{relic.Grade.ToString().ToLower()}");
            string description = relic.Effect.GetDescription();
            _nameText.text = name;
            _gradeText.text = grade;
            _gradeText.color = Utils.GetRelicGradeColor(relic.Grade);
            _descriptionText.text = description;
            _selectedRelic = (container, relic, isReward);
        }

        private void OnRerollButtonClicked()
        {
            if (GameManager.Instance.GameState.RelicRewardRerollCount.Value <= 0)
            {
                ConfirmationController.Instance.ShowWarning(Utils.GetUIString("relic-reward_no-reroll-count"));
                return;
            }

            try
            {
                AddToAlreadyRolledRelics(_relicRewards);
                GameManager.Instance.GameState.RelicRewardRerollCount.Value--;
                GameManager.Instance.RelicReward(_weight, _alreadyRolledRelics);
            }
            catch (Exception e)
            {
                ConfirmationController.Instance.ShowWarning(Utils.GetUIString("unkown-error"));
                Debug.LogError(e);
            }
        }

        private void OnAcquireDiscardButtonClicked()
        {
            if (_selectedRelic.relic == null)
            {
                ConfirmationController.Instance.ShowWarning(Utils.GetUIString("relic-reward_no-relic-selected"));
                return;
            }

            if (_selectedRelic.isReward)
            {    
                if (GameManager.Instance.GameState.OwnedRelics.Value.Count >= PersistentGameState.Instance.GetMaxRelicCount())
                {
                    ConfirmationController.Instance.ShowWarning(Utils.GetUIString("relic-reward_max-relic-slot"));
                    return;
                }

                try
                {
                    GameManager.Instance.AcquireRelic(_selectedRelic.relic.EffectName);
                    Hide();
                }
                catch (Exception e)
                {
                    ConfirmationController.Instance.ShowWarning(Utils.GetUIString("unkown-error"));
                    Debug.LogError(e);
                }
            }
            else
            {
                DiscardRelic(_selectedRelic.relic);
            }
        }

        private void DiscardRelic(Relic relic)
        {
            ConfirmationController.Instance.ShowConfirmation(
                Utils.GetUIString("relic-reward_discard-relic-confirmation", new object[] { relic.Effect.GetName() }),
                () => {
                    try
                    {
                        GameManager.Instance.DiscardRelic(relic.EffectName);
                        AddToAlreadyRolledRelics(new[] { relic });
                        RefreshOwnedRelics();
                        SelectRelic(null, null, true);
                    }
                    catch (Exception e)
                    {
                        ConfirmationController.Instance.ShowWarning(Utils.GetUIString("unkown-error"));
                        Debug.LogError(e);
                    }
                });
        }

        private void AddToAlreadyRolledRelics(IEnumerable<Relic> relics)
        {
            var relicsToAddToAlreadyRolledRelics = relics.Where(r => !_alreadyRolledRelics.Any(ar => ar.EffectName == r.EffectName));
            if (relicsToAddToAlreadyRolledRelics.Any())
            {
                _alreadyRolledRelics = _alreadyRolledRelics.Concat(relicsToAddToAlreadyRolledRelics).ToArray();
            }
        }

        private void OnSkipRewardButtonClicked()
        {
            Hide();
        }
    }
}