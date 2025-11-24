using LoopLegacy.Loader;
using LoopLegacy.Manager;
using LoopLegacy.UI.Component;
using LoopLegacy.State;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace LoopLegacy.UI.Controller
{
    public class CodexController : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _monsterButton;
        [SerializeField] private Button _weaponButton;
        [SerializeField] private Button _armorButton;
        [SerializeField] private Button _relicButton;
        [SerializeField] private CodexDetailController _codexDetailController;
        [SerializeField] private VirtualizedScrollRect _virtualizedScrollRect;

        private Action _onClose;
        private List<object> _currentDataList = new List<object>(); // 현재 표시할 데이터 리스트
        private string _currentCodexType = "";
        private int _selectedIndex = -1;

        void Awake()
        {
            SetupVirtualizedScrollRect();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {            
            _monsterButton.onClick.AddListener(() => PopulateCodexList("Monster"));
            _weaponButton.onClick.AddListener(() => PopulateCodexList("Weapon"));
            _armorButton.onClick.AddListener(() => PopulateCodexList("Armor"));
            _relicButton.onClick.AddListener(() => PopulateCodexList("Relic"));

            _backButton.onClick.AddListener(OnBackButtonClicked);
        }

        public void Show(Action onClose)
        {
            gameObject.SetActive(true);
            PopulateCodexList("Monster");
            _onClose = onClose;
        }

        public void Hide(bool invokeOnClose = true)
        {
            if (invokeOnClose)
            {
                _onClose?.Invoke();
                _onClose = null;
            }
            gameObject.SetActive(false);
        }

        private void OnBackButtonClicked()
        {
            Hide();
        }

        private void SetupVirtualizedScrollRect()
        {
            // VirtualizedScrollRect의 OnItemUpdate 이벤트에 데이터 바인딩 함수 연결
            _virtualizedScrollRect.OnItemUpdate += OnItemUpdate;
        }

        private void OnItemUpdate(GameObject element, object data, int index)
        {
            var listElement = element.GetComponent<ListElement>();
            listElement.ToggleSelect(_selectedIndex == index);

            switch (_currentCodexType)
            {
                case "Monster":
                    UpdateMonsterElement(listElement, data as MonsterData, index);
                    break;

                case "Weapon":
                case "Armor":
                    UpdateEquipmentElement(listElement, data as EquipmentData, index);
                    break;

                case "Relic":
                    UpdateRelicElement(listElement, data as RelicData, index);
                    break;
            }
        }

        private void UpdateMonsterElement(ListElement listElement, MonsterData monster, int index)
        {
            int killCount = PersistentGameState.Instance.CodexState.GetMobKillCount(monster.code);
            if (killCount == 0)
            {
                listElement.UpdateElement(
                    null,
                    "???",
                    "",
                    $"No. {index + 1}",
                    () => OnMonsterElementClicked(monster, index));
            }
            else
            {
                bool[] icons = null;
                if (monster.drops.Any())
                {
                    bool allCollected = true;
                    foreach (var drop in monster.drops)
                    {
                        if (drop.itemType == DropType.Relic)
                        {
                            int relicId = TableManager.GetRelicId(drop.relicEffectName);
                            if (PersistentGameState.Instance.CodexState.GetRelic(relicId) is { } relic)
                            {
                                if (drop.relicLevel > relic.Level)
                                {
                                    allCollected = false;
                                }
                            }
                            else
                            {
                                allCollected = false;
                            }
                        }
                        else if (drop.itemType == DropType.Weapon || drop.itemType == DropType.Armor)
                        {
                            EquipmentType equipmentType = drop.itemType == DropType.Weapon ? EquipmentType.Weapon : EquipmentType.Armor;
                            if (PersistentGameState.Instance.InventoryState.GetOwnedEquipments(equipmentType)[drop.itemId] < InventoryState.MAX_EQUIPMENT_DUPLICATE_COUNT)
                            {
                                allCollected = false;
                            }
                        }
                    }

                    if (allCollected)
                    {
                        icons = new bool[] { false, true };
                    }
                    else
                    {
                        icons = new bool[] { true, false };
                    }
                }

                listElement.UpdateElement(
                    monster.sprite,
                    Utils.GetMonsterName(monster),
                    "",
                    $"No. {index + 1}",
                    () => OnMonsterElementClicked(monster, index),
                    icons: icons);
            }
        }

        private void UpdateEquipmentElement(ListElement listElement, EquipmentData equipmentData, int index)
        {
            var count = PersistentGameState.Instance.InventoryState.GetOwnedEquipments(equipmentData.type)[equipmentData.id];
            if (count == 0)
            {
                listElement.UpdateElement(
                    null,
                    "???",
                    "",
                    $"No. {index + 1}",
                    () => OnEquipmentElementClicked(equipmentData, index));
            }
            else
            {
                bool[] icons = null;
                if (count == InventoryState.MAX_EQUIPMENT_DUPLICATE_COUNT)
                {
                    icons = new bool[] { false, true };
                }

                listElement.UpdateElement(
                    equipmentData.sprite,
                    Utils.GetEquipmentName(equipmentData.type, equipmentData.id),
                    $"{equipmentData.baseValue} (+{equipmentData.multiplier}%)",
                    $"No. {index + 1}",
                    () => OnEquipmentElementClicked(equipmentData, index),
                    icons: icons);
            }
        }

        private void UpdateRelicElement(ListElement listElement, RelicData relicData, int index)
        {
            var relic = PersistentGameState.Instance.CodexState.GetRelic(relicData.id);
            if (relic == null)
            {
                listElement.UpdateElement(
                    null,
                    "???",
                    "",
                    $"No. {index + 1}",
                    () => OnRelicElementClicked(relicData, index));
            }
            else
            {
                listElement.UpdateElement(
                    relicData.sprite,
                    Utils.GetRelicName(relicData.effectName),
                    "",
                    $"No. {index + 1}",
                    () => OnRelicElementClicked(relicData, index));
            }
        }

        private void PopulateCodexList(string codexType)
        {
            _selectedIndex = -1;
            _codexDetailController.UpdateCodexDetail(null, "", "");
            _monsterButton.interactable = true;
            _weaponButton.interactable = true;
            _armorButton.interactable = true;
            _relicButton.interactable = true;
            
            // 데이터 리스트 설정
            _currentDataList.Clear();
            _currentCodexType = codexType;
            
            switch (codexType)
            {
                case "Monster":
                    _monsterButton.interactable = false;
                    foreach (var monster in TableManager.GetAllMonsters())
                    {
                        _currentDataList.Add(monster);
                    }
                    foreach (var boss in TableManager.GetAllBosses())
                    {
                        _currentDataList.Add(boss);
                    }
                    break;

                case "Weapon":
                    _weaponButton.interactable = false;
                    foreach (var weapon in TableManager.GetEquipments(EquipmentType.Weapon))
                    {
                        _currentDataList.Add(weapon);
                    }
                    break;

                case "Armor":
                    _armorButton.interactable = false;
                    foreach (var armor in TableManager.GetEquipments(EquipmentType.Armor))
                    {
                        _currentDataList.Add(armor);
                    }
                    break;

                case "Relic":
                    _relicButton.interactable = false;
                    foreach (var relic in TableManager.GetAllRelics())
                    {
                        _currentDataList.Add(relic);
                    }
                    break;
            }
            
            // VirtualizedScrollRect에 데이터 설정
            _virtualizedScrollRect.SetData(_currentDataList);
        }

        private void OnMonsterElementClicked(MonsterData monster, int index)
        {
            _selectedIndex = index;
            _virtualizedScrollRect.ForceUpdate();
            var killCount = PersistentGameState.Instance.CodexState.GetMobKillCount(monster.code);
            string killCountString = Utils.GetUIString("kill-count", new object[] { killCount });
            if (killCount == 0)
            {
                _codexDetailController.UpdateCodexDetail(null, "???", "");
            }
            else
            {
                string dropText = "";
                foreach (var drop in monster.drops)
                {
                    var name = drop.itemType == DropType.Relic ?
                        Utils.GetRelicName(drop.relicEffectName) + $" Lv. {drop.relicLevel + 1}" :
                        Utils.GetEquipmentName(
                            drop.itemType == DropType.Weapon ? EquipmentType.Weapon : EquipmentType.Armor,
                            drop.itemId);
                    dropText += $"{name} - {drop.dropRate * 100:F1}%\n";
                }
                string descriptionText =
                    $"Lv.{monster.level}\n" +
                    $"{killCountString}: {killCount}\n" +
                    $"{GetMonsterDetailText(monster, killCount)}\n" +
                    $"{Utils.GetUIString("drops")}\n" +
                    $"{dropText}";
                if (LibraryManager.IsLibraryUnlocked())
                {
                    descriptionText += $"\n{Utils.GetUIString("library_monster-killcount-effect")}: DROP +{(int)(LibraryManager.GetMonsterDropMultiplier(killCount) * 100 - 100)}%";
                }

                _codexDetailController.UpdateCodexDetail(
                    monster.sprite,
                    Utils.GetMonsterName(monster),
                    descriptionText);
            }
        }

        private string GetMonsterDetailText(MonsterData monster, int killCount)
        {
            string detailText = "";
            detailText += $"HP: {monster.hp}\n";
            detailText += $"ATK: {monster.atk}\n";
            detailText += $"EXP: {monster.exp}\n";
            detailText += $"GOLD: {monster.gold}\n";

            return detailText;
        }

        private void OnEquipmentElementClicked(EquipmentData equipmentData, int index)
        {
            _selectedIndex = index;
            _virtualizedScrollRect.ForceUpdate();
            var count = PersistentGameState.Instance.InventoryState.GetOwnedEquipments(equipmentData.type)[equipmentData.id];
            if (count == 0)
            {
                _codexDetailController.UpdateCodexDetail(null, "???", "");
            }
            else
            {
                string descriptionText = $"{equipmentData.baseValue} (+{equipmentData.multiplier}%)\n" +
                    $"{Utils.GetUIString("owned-count")}: {count}\n" +
                    $"{Utils.GetEquipmentDescription(equipmentData.type, equipmentData.id)}";
                _codexDetailController.UpdateCodexDetail(
                    equipmentData.sprite,
                    Utils.GetEquipmentName(equipmentData.type, equipmentData.id),
                    descriptionText);
            }
        }

        private void OnRelicElementClicked(RelicData relicData, int index)
        {
            _selectedIndex = index;
            _virtualizedScrollRect.ForceUpdate();
            var relic = PersistentGameState.Instance.CodexState.GetRelic(relicData.id);
            if (relic == null)
            {
                _codexDetailController.UpdateCodexDetail(null, "???", "");
            }
            else
            {
                string descriptionText = $"Lv. {relic.Level + 1}\n";
                descriptionText += $"<color={Utils.GetRelicGradeColorHex(relic.Grade)}>{Utils.GetUIString("grade_" + relic.Grade.ToString().ToLowerInvariant())}</color>";
                string[] args = new string[0];
                if (relicData.values.Length > 0)
                {
                    var value0 = relicData.values[0];
                    args = new string[value0.Split(':').Length];
                    for (int i = 0; i < args.Length; i++)
                    {
                        args[i] = string.Empty;
                    }
                    foreach (var (i, value) in relicData.values.Select((value, index) => (index, value)))
                    {
                        foreach (var (j, f) in value.Split(':').Select((value, index) => (index, float.Parse(value))))
                        {
                            if (i == relic.Level)
                            {
                                args[j] += $"{f}/";
                            }
                            else
                            {
                                args[j] += $"<color=#808080>{f}</color>/";
                            }
                        }
                    }
                    for (int i = 0; i < args.Length; i++)
                    {
                        args[i] = args[i].TrimEnd('/');
                    }
                    descriptionText += $"\n\n{Utils.GetRelicDescription(relicData.effectName, args)}";
                }
                _codexDetailController.UpdateCodexDetail(
                    relicData.sprite,
                    Utils.GetRelicName(relicData.effectName),
                    descriptionText);
            }
        }
    }
}