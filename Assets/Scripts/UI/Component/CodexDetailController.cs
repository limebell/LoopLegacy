using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoopLegacy.UI.Component
{
    public class CodexDetailController : MonoBehaviour
    {
        [SerializeField] private Image _codexImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _descriptionText;

        void Start()
        {
            UpdateCodexDetail(null, "", "");
        }

        public void UpdateCodexDetail(Sprite sprite, string name, string description)
        {
            _codexImage.sprite = sprite;
            _codexImage.color = new Color(1, 1, 1, sprite == null ? 0 : 1);
            if (sprite != null)
            {
                int preferredWidth = Mathf.Max((int)sprite.rect.width, 32);
                int preferredHeight = Mathf.Max((int)sprite.rect.height, 32);
                // sprite 비율에 맞게 조정
                if (sprite.rect.width > sprite.rect.height)
                {
                    _codexImage.rectTransform.localScale = new Vector3(1, sprite.rect.height / sprite.rect.width, 1) * (sprite.rect.width / preferredWidth);
                }
                else if (sprite.rect.width < sprite.rect.height)
                {
                    _codexImage.rectTransform.localScale = new Vector3(sprite.rect.width / sprite.rect.height, 1, 1) * (sprite.rect.height / preferredHeight);
                }
                else
                {
                    _codexImage.rectTransform.localScale = Vector3.one * (sprite.rect.width / preferredWidth);
                }
            }
            _nameText.text = name;
            _descriptionText.text = description;
        }
    }
}
