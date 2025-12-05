using TMPro;
using UnityEngine;

namespace LoopLegacy.UI.Component
{
    public class DamageText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _damageText;
        [SerializeField] private TextMeshProUGUI _critText;
        [SerializeField] private TextMeshProUGUI _comboText;

        public void SetDamage(int damage, DamageTextType type, int combo)
        {
            _damageText.text = type switch
            {
                DamageTextType.Evasion => "Dodge!",
                DamageTextType.Execution => "Execution!",
                DamageTextType.Heal => $"+{damage}",
                _ => $"-{damage}",
            };
            _critText.gameObject.SetActive(type == DamageTextType.Critical || type == DamageTextType.Reflect);
            _comboText.gameObject.SetActive(combo > 1);
            _comboText.text = $"{combo} combo";
            _critText.text = type switch
            {
                DamageTextType.Critical => "Critical!",
                DamageTextType.Reflect => "Reflect!",
                _ => "",
            };
            
            _critText.color = type switch
            {
                DamageTextType.Critical => Color.red,
                DamageTextType.Reflect => Color.orange,
                _ => Color.white,
            };

            _damageText.color = type switch
            {
                DamageTextType.Critical => Color.red,
                DamageTextType.Evasion => Color.skyBlue,
                DamageTextType.Execution => Color.red,
                DamageTextType.Heal => Color.green,
                _ => Color.white,
            };
        }
    }
}
