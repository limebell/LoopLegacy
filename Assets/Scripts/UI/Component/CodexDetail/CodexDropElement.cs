using LoopLegacy;
using LoopLegacy.Battle;
using LoopLegacy.Loader;
using LoopLegacy.Manager;
using LoopLegacy.State;
using LoopLegacy.UI.Controller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodexDropElement : MonoBehaviour
{
    [SerializeField] private Image _itemImage;
    [SerializeField] private TMP_Text _itemNameText;
    [SerializeField] private TMP_Text _dropRateText;
    [SerializeField] private Image _checkIcon;

    private DropEntry _drop;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void UpdateElement(DropEntry dropEntry)
    {
        _drop = dropEntry;
        switch (_drop.itemType)
        {
            case DropType.Weapon:
            {
                var weapon = TableManager.GetEquipment(EquipmentType.Weapon, _drop.itemId);
                _itemImage.sprite = weapon.sprite;
                _itemNameText.text = Utils.GetEquipmentName(EquipmentType.Weapon, _drop.itemId);
                _checkIcon.gameObject.SetActive(
                    PersistentGameState.Instance.InventoryState.GetOwnedEquipments(
                        EquipmentType.Weapon)[_drop.itemId] == InventoryState.MAX_EQUIPMENT_DUPLICATE_COUNT);
                break;
            }
            case DropType.Armor:
            {
                var armor = TableManager.GetEquipment(EquipmentType.Armor, _drop.itemId);
                _itemImage.sprite = armor.sprite;
                _itemNameText.text = Utils.GetEquipmentName(EquipmentType.Armor, _drop.itemId);
                _checkIcon.gameObject.SetActive(
                    PersistentGameState.Instance.InventoryState.GetOwnedEquipments(
                        EquipmentType.Armor)[_drop.itemId] == InventoryState.MAX_EQUIPMENT_DUPLICATE_COUNT);
                break;
            }
            case DropType.Relic:
            {
                var relic = TableManager.GetRelic(_drop.relicEffectName);
                _itemImage.sprite = relic.sprite;
                _itemNameText.text = Utils.GetRelicName(_drop.relicEffectName) + $" Lv. {_drop.relicLevel + 1}";
                _checkIcon.gameObject.SetActive(
                    (PersistentGameState.Instance.CodexState.GetRelic(relic.effectName)?.Level ?? -1) >= _drop.relicLevel);
                break;
            }
            default:
            {
                _itemImage.sprite = null;
                break;
            }
        }
        _dropRateText.text = $"{_drop.dropRate * 100:F1}%";
    }

    public void OnClick()
    {
        if (_drop == null)
        {
            return;
        }

        switch (_drop.itemType)
        {
            case DropType.Weapon:
            {
                var equipment = TableManager.GetEquipment(EquipmentType.Weapon, _drop.itemId);
                ConfirmationController.Instance.ShowEquipment(equipment);
                break;
            }
            case DropType.Armor:
            {
                var equipment = TableManager.GetEquipment(EquipmentType.Armor, _drop.itemId);
                ConfirmationController.Instance.ShowEquipment(equipment);
                break;
            }
            case DropType.Relic:
            {
                var relic = TableManager.GetRelic(_drop.relicEffectName);
                ConfirmationController.Instance.ShowRelic(new Relic(relic, _drop.relicLevel));
                break;
            }
        }
    }
}
