using System.Collections.Generic;
using LoopLegacy.Battle;
using LoopLegacy.Manager;
using LoopLegacy.State;
using LoopLegacy.UI.Component;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoopLegacy.UI.Controller
{
    public class GameOverController : MonoBehaviour
    {
        private const int MAX_VISIBLE_DROPS_COUNT = 10;

        [SerializeField] private TMP_Text _commentText;
        [SerializeField] private GameObject _dropElementPrefab;
        [SerializeField] private Transform _dropElementContainer;
        [SerializeField] private TMP_Text _extraCountText;
        [SerializeField] private TMP_Text _summaryText;
        [SerializeField] private Button _advertiseButton;
        [SerializeField] Button _territoryButton;

        private List<DropElement> _dropElements = new List<DropElement>();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _advertiseButton.onClick.AddListener(OnAdvertiseButtonClicked);
            _territoryButton.onClick.AddListener(OnTerritoryButtonClicked);
        }

        public void Show(
            int generation,
            DropEntryData[] droppedItems,
            CombatInfo combatInfo,
            int level,
            int gold
        )
        {
            gameObject.SetActive(true);
            string commentary = Utils.GetUIString("game-over_commentary", new object[] { Utils.Ordinal(generation) });
            _commentText.text = commentary;

            // Update dropped items
            Debug.Log($"Dropped items: {droppedItems.Length}");
            _dropElements.ForEach(dropElement => Destroy(dropElement.gameObject));
            _dropElements.Clear();
            int droppedCount = 0;
            foreach (DropEntryData dropEntryData in droppedItems)
            {
                Debug.Log($"Drop entry data: {dropEntryData.itemType} {dropEntryData.itemId} {dropEntryData.relicLevel} {dropEntryData.count}");
                droppedCount++;
                if (droppedCount > MAX_VISIBLE_DROPS_COUNT)
                {
                    break;
                }

                var dropElement = Instantiate(_dropElementPrefab, _dropElementContainer);
                dropElement.gameObject.SetActive(true);
                _dropElements.Add(dropElement.GetComponent<DropElement>());
                if (dropEntryData.itemType == DropType.Relic)
                {
                    Debug.Log($"Relic: {dropEntryData.itemId} {dropEntryData.relicLevel} {dropEntryData.count}");
                    var relicData = TableManager.GetRelic(dropEntryData.itemId);
                    var image = relicData.sprite;
                    dropElement.GetComponent<DropElement>().UpdateElement(
                        image: image,
                        type: "UNLOCK",
                        isNew: true,
                        count: dropEntryData.count,
                        onClick: () => ConfirmationController.Instance.ShowRelic(new Relic(relicData, dropEntryData.relicLevel)));
                }
                else
                {
                    Debug.Log($"Equipment: {dropEntryData.itemType} {dropEntryData.itemId} {dropEntryData.relicLevel} {dropEntryData.count}");
                    var equipment = TableManager.GetEquipment(dropEntryData.itemType switch
                    {
                        DropType.Weapon => EquipmentType.Weapon,
                        DropType.Armor => EquipmentType.Armor,
                        _ => EquipmentType.Weapon,
                    }, dropEntryData.itemId);
                    var image = equipment.sprite;
                    var isNew = PersistentGameState.Instance.InventoryState.GetOwnedEquipments(equipment.type)[equipment.id] == dropEntryData.count;
                    dropElement.GetComponent<DropElement>().UpdateElement(
                        image: image,
                        type: "DROP",
                        isNew: isNew,
                        count: dropEntryData.count,
                        onClick: () => ConfirmationController.Instance.ShowEquipment(equipment));
                }
            }

            if (droppedCount > MAX_VISIBLE_DROPS_COUNT)
            {
                _extraCountText.text = Utils.GetUIString("extra-drops", new object[] {droppedCount - MAX_VISIBLE_DROPS_COUNT});
            }
            else
            {
                _extraCountText.gameObject.SetActive(false);
            }

            // Update summary
            string combatSummaryString = Utils.GetUIString(
                "game-over_combat-summary",
                new object[] { combatInfo.CombatCount, combatInfo.Wins, combatInfo.Losses, combatInfo.DefeatedBossesCount });
            string finalLevelString = Utils.GetUIString("game-over_final-level");
            string earnedGoldString = Utils.GetUIString("game-over_earned-gold");
            _summaryText.text = $"{combatSummaryString}\n{finalLevelString}: {level:n0}\n{earnedGoldString}: {gold:n0}G";
            _advertiseButton.gameObject.SetActive(!GameManager.Instance.GameState.IsAdvertised);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            RewardedAdManager.Instance.DestroyRewardedAd();
        }

        private void OnAdvertiseButtonClicked()
        {
            if (GameManager.Instance.GameState.IsAdvertised)
            {
                ConfirmationController.Instance.ShowWarning(Utils.GetUIString("game-over_advertise_already_advertised"));
                return;
            }

#if UNITY_ANDROID || UNITY_IOS

            if (!RewardedAdManager.Instance.IsLoaded.Value)
            {
                ConfirmationController.Instance.ShowWarning(Utils.GetUIString("game-over_advertise_not_ready"));
                return;
            }

            if (Debug.isDebugBuild)
            {
                RewardAdvertise();
            }
            else
            {
                RewardedAdManager.Instance.ShowRewardedAd(_ =>
                {
                    RewardAdvertise();
                });
            }
#else
            RewardAdvertise();
#endif
        }

        private void RewardAdvertise()
        {
            GameManager.Instance.GameState.PlayerStats.BattlePoint.Value = 5;
            GameManager.Instance.GameState.IsAdvertised = true;
            GameManager.Instance.Save();
            Hide();
        }

        private void OnTerritoryButtonClicked()
        {
            GameManager.Instance.EndLoop();
            FadeController.LoadScene("Territory", FadeController.MAP_MOVE_DELAY);
        }
    }
}
