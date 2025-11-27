using LoopLegacy.Loader;
using TMPro;
using UnityEngine;

namespace LoopLegacy.UI.Component.CodexDetail
{
    public class EquipmentCodexDetail : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _descriptionText;

        public void UpdateCodexDetail(string name, string description)
        {
            _nameText.text = name;
            _descriptionText.text = description;
        }
    }
}