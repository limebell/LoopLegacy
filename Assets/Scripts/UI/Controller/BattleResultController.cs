using LoopLegacy.Battle;
using LoopLegacy.Loader;
using LoopLegacy.Manager;
using LoopLegacy.State;
using LoopLegacy.UI.Component;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoopLegacy.UI.Controller
{
    public class BattleResultController : MonoBehaviour
    {
        [SerializeField] private TMP_Text _resultLabel;
        [SerializeField] private TMP_Text _battlePointLabel;
        [SerializeField] private TMP_Text _levelLabel;
        [SerializeField] private TMP_Text _goldLabel;
        [SerializeField] private GameObject _dropContainer;
        [SerializeField] private DropElement[] _drops;
        [SerializeField] private GameObject _dropDialogue;
        [SerializeField] private TMP_Text _dropDialogueText;
        [SerializeField] private AudioClip _victoryBGM;

        void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnClickBattleResult);
        }

        private void OnClickBattleResult()
        {
            BattleManager.Instance.OnClickBattleUI();
        }

        public void ShowBattleResult(BattleContext result)
        {
            gameObject.SetActive(true);
            AudioManager.Instance.StopBGM();
            var currentBattlePoint = GameManager.Instance.GameState.PlayerStats.BattlePoint.Value;
            _dropContainer.SetActive(result.IsVictory);
            if (result.IsVictory)
            {
                var currentLevel = GameManager.Instance.GameState.PlayerStats.Level.Value;
                var expectedLevel = GameManager.Instance.GameState.PlayerStats.ExpectedLevel(result.EarnedEXP);
                _resultLabel.text = Utils.GetUIString("victory");
                _levelLabel.text = $"{currentLevel}  >  <color=green>{expectedLevel}</color>";
                _goldLabel.text = $"{PersistentGameState.Instance.Gold.Value:n0}G  <color=green>+{result.EarnedGold:n0}G</color>";
                
                _dropDialogue.SetActive(result.DroppedItems.Count > 0);
                _dropDialogueText.text = string.Join("\n", result.DroppedItems.Select(drop => GetDropDialogueText(drop)));

                for (int i = 0; i < _drops.Length; i++)
                {
                    _drops[i].gameObject.SetActive(i < result.DroppedItems.Count);
                    if (i < result.DroppedItems.Count)
                    {
                        var drop = result.DroppedItems[i];
                        if (drop.itemType == DropType.Relic)
                        {
                            var relicId = TableManager.GetRelicId(drop.relicEffectName);
                            var relicData = TableManager.GetRelic(relicId);
                            _drops[i].UpdateElement(
                                image: relicData.sprite,
                                type: "UNLOCK",
                                isNew: true,
                                count: 1,
                                onClick: () => ConfirmationController.Instance.ShowRelic(new Relic(relicData, drop.relicLevel)));
                        }
                        else
                        {
                            var equipmentData = TableManager.GetEquipment(drop.itemType switch
                            {
                                DropType.Weapon => EquipmentType.Weapon,
                                DropType.Armor => EquipmentType.Armor,
                                _ => EquipmentType.Weapon,
                            }, drop.itemId);
                            var isNew = PersistentGameState.Instance.InventoryState.GetOwnedEquipments(equipmentData.type)[equipmentData.id] == 0;
                            _drops[i].UpdateElement(
                                image: equipmentData.sprite,
                                type: "DROP",
                                isNew: isNew,
                                count: 1,
                                onClick: () => ConfirmationController.Instance.ShowEquipment(equipmentData));
                        }
                    }
                }

                // 승리 BGM 재생 (루프 없음)
                AudioManager.Instance.PlayBGM(_victoryBGM, false);
            }
            else
            {
                _resultLabel.text = Utils.GetUIString("defeat");
                _levelLabel.text = "-";
                _goldLabel.text = "-";
            }

            var battlePointAfter = Mathf.Max(0, currentBattlePoint + result.EarnedBattlePoint);
            var color = battlePointAfter < currentBattlePoint ? "red" : "green";
            _battlePointLabel.text = $"{currentBattlePoint}  >  <color={color}>{battlePointAfter}</color>";
        }

        private string GetDropDialogueText(DropEntry drop)
        {
            if (drop.itemType == DropType.Relic)
            {
                var relicId = TableManager.GetRelicId(drop.relicEffectName);
                var relicName = $"{Utils.GetRelicName(TableManager.GetRelic(relicId).effectName)} Lv. {drop.relicLevel + 1}";
                return Utils.GetUIString("drop-dialogue_relic", new object[] { relicName });
            }
            else
            {
                var equipmentName = Utils.GetEquipmentName(TableManager.GetEquipment(drop.itemType switch
                {
                    DropType.Weapon => EquipmentType.Weapon,
                    DropType.Armor => EquipmentType.Armor,
                    _ => EquipmentType.Weapon,
                }, drop.itemId).type, drop.itemId);
                return Utils.GetUIString(
                    drop.itemType == DropType.Weapon ? "drop-dialogue_weapon" : "drop-dialogue_armor",
                    new object[] { equipmentName });
            }
        }

        public void HideBattleResult()
        {
            gameObject.SetActive(false);
            AudioManager.Instance.StopBGM();
            AudioManager.Instance.RestoreBGMState(); // 맵 배경음악 복원
        }
    }
}