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
            _critText.gameObject.SetActive(type == DamageTextType.Critical);
            _comboText.gameObject.SetActive(combo > 1);
            _comboText.text = $"{combo} combo";

            _damageText.color = type switch
            {
                DamageTextType.Critical => Color.red,
                DamageTextType.Evasion => Color.lightCoral,
                DamageTextType.Execution => Color.red,
                DamageTextType.Heal => Color.green,
                _ => Color.white,
            };
        }
    }
}
