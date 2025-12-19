using System.Linq;
using LoopLegacy;
using LoopLegacy.Loader;
using LoopLegacy.Manager;
using LoopLegacy.State;
using TMPro;
using UnityEngine;

public class LibraryController : MonoBehaviour
{
    [SerializeField] private TMP_Text _collectionLabelText;
    [SerializeField] private TMP_Text _collectionValueText;
    [SerializeField] private TMP_Text _effectLabelText;
    [SerializeField] private TMP_Text _effectValueText;

    public void Show()
    {
        gameObject.SetActive(true);
        
        int weaponCount = TableManager.GetEquipments(EquipmentType.Weapon).Count;
        int armorCount = TableManager.GetEquipments(EquipmentType.Armor).Count;
        _collectionLabelText.text =
            $"{Utils.GetUIString("accumulated-level")}\n" +
            $"{Utils.GetUIString("library_defeated-boss-count")}\n\n" +
            $"{Utils.GetUIString("library_collected-weapon")}\n" +
            $"{Utils.GetUIString("library_fully-collected-weapon")}\n\n" +
            $"{Utils.GetUIString("library_collected-armor")}\n" +
            $"{Utils.GetUIString("library_fully-collected-armor")}";
        _collectionValueText.text =
            $"{PersistentGameState.Instance.AccumulatedLevel:n0}\n" +
            $"{PersistentGameState.Instance.SlainedBosses.Count:n0}\n\n" +
            $"{PersistentGameState.Instance.InventoryState.GetOwnedEquipments(EquipmentType.Weapon).Count(e => e > 0)}/{weaponCount}\n" +
            $"{LibraryManager.FullyCollectionCount(EquipmentType.Weapon)}/{weaponCount}\n\n" +
            $"{PersistentGameState.Instance.InventoryState.GetOwnedEquipments(EquipmentType.Armor).Count(e => e > 0)}/{armorCount}\n" +
            $"{LibraryManager.FullyCollectionCount(EquipmentType.Armor)}/{armorCount}";
        _effectLabelText.text =
            $"{Utils.GetUIString("library_accumulated-level-bonus")}\n" +
            $"{Utils.GetUIString("library_stat-multiplier-by-accumulated-level-and-defeated-boss-count")}\n" +
            $"{Utils.GetUIString("library_fully-collected-equipments")}\n" +
            $"{Utils.GetUIString("library_stat-boost-per-fully-collected-equipment")}\n" +
            $"{Utils.GetUIString("library_stat-boost-by-fully-collected-equipment")}\n\n" +
            $"{Utils.GetUIString("library_monster-drop-multiplier")}\n" +
            $" - {Utils.GetUIString("library_monster-drop-multiplier-by-kill-count", new object[] { 50 })}\n" +
            $" - {Utils.GetUIString("library_monster-drop-multiplier-by-kill-count", new object[] { 100 })}\n" +
            $" - {Utils.GetUIString("library_monster-drop-multiplier-by-kill-count", new object[] { 200 })}";
        int fullyCollectedEquipmentCount = LibraryManager.FullyCollectionCount(EquipmentType.Weapon) + LibraryManager.FullyCollectionCount(EquipmentType.Armor);
        _effectValueText.text =
            $"{LibraryManager.GetAccumulatedLevelBonus(PersistentGameState.Instance.AccumulatedLevel)}\n" +
            $"{Mathf.RoundToInt(LibraryManager.GetStatMultiplier() * 100 - 100)}%\n" +
            $"{fullyCollectedEquipmentCount}\n" +
            $"{PersistentGameState.Instance.HouseState.GetUpgradeValue(UpgradeType.LibraryManagement):n0}\n" +
            $"{LibraryManager.GetStatBoost():n0}\n\n\n" +
            $"{Mathf.RoundToInt(LibraryManager.GetMonsterDropMultiplier(50) * 100 - 100)}%\n" +
            $"{Mathf.RoundToInt(LibraryManager.GetMonsterDropMultiplier(100) * 100 - 100)}%\n" +
            $"{Mathf.RoundToInt(LibraryManager.GetMonsterDropMultiplier(200) * 100 - 100)}%";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
