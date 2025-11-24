using LoopLegacy.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace LoopLegacy.UI.Component
{
    public class PlaySoundOnClick : MonoBehaviour
    {
        [SerializeField]
        private AudioClip _audioClip;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
        }

        void OnClick()
        {
            AudioManager.Instance.PlaySFXSound(_audioClip);
        }
    }
}
