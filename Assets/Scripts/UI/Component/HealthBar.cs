using TMPro;
using UnityEngine;

namespace LoopLegacy.UI.Component
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Gauge _gauge;

        public void UpdateHP(int currentHP, int maxHP, bool ignoreBuffer = false)
        {
            float percentage = Mathf.Clamp01((float)currentHP / maxHP);
            _gauge.SetValue(percentage, ignoreBuffer);
            _text.text = $"{currentHP:n0}/{maxHP:n0}";
        }
    }
}
