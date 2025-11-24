using DG.Tweening;
using System.Collections;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace LoopLegacy.UI.Controller
{
    public class FadeController : MonoBehaviour
    {
        public const float MAP_MOVE_DELAY = 0.4f;
        public static FadeController Instance { get; private set; }

        private bool _isLoading = false;

        [SerializeField]
        private Image _loadingElement;
        [SerializeField]
        private TMP_Text _loadingText;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _loadingElement.gameObject.SetActive(false);
            _loadingText.gameObject.SetActive(false);
        }

        public static void LoadScene(
            string sceneName,
            float delay = 0,
            Action onLoadComplete = null,
            Action onFadeComplete = null)
        {
            LoadSceneWithMiddleScene(sceneName, "", delay, onLoadComplete, onFadeComplete);
        }

        public static void LoadSceneWithMiddleScene(
            string sceneName,
            string middleSceneName,
            float delay = 0,
            Action onLoadComplete = null,
            Action onFadeComplete = null)
        {
            if (Instance == null)
            {
                Debug.LogError("FadeHUDController: Instance is null");
                if (middleSceneName != "")
                {
                    SceneManager.LoadScene(middleSceneName);
                }

                SceneManager.LoadScene(sceneName);

                onLoadComplete?.Invoke();
                onFadeComplete?.Invoke();
                return;
            }

            if (Instance._isLoading)
            {
                Debug.LogError("FadeHUDController: Already loading scene");
                return;
            }

            Instance._isLoading = true;
            Instance._loadingElement.gameObject.SetActive(true);
            Instance._loadingText.gameObject.SetActive(true);

            DOTween.To(() => Instance._loadingElement.color.a, x => {
                        Instance._loadingElement.color = new Color(0, 0, 0, x);
                        Instance._loadingText.color = new Color(1, 1, 1, x);
                    }, 1, 0.2f)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    Instance.StartCoroutine(Instance.LoadSceneAsync(sceneName, middleSceneName, delay, onLoadComplete, onFadeComplete));
                });
        }

        private IEnumerator LoadSceneAsync(
            string sceneName,
            string middleSceneName,
            float delay,
            Action onLoadComplete,
            Action onFadeComplete)
        {
            if (middleSceneName != "")
            {
                var asyncLoadMiddle = SceneManager.LoadSceneAsync(middleSceneName);

                while (!asyncLoadMiddle.isDone)
                {
                    yield return new WaitForFixedUpdate();
                }
            }

            var asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            while (!asyncLoad.isDone)
            {
                yield return new WaitForFixedUpdate();
            }
            
            onLoadComplete?.Invoke();

            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            Instance._loadingText.gameObject.SetActive(false);
            DOTween.To(() => _loadingElement.color.a, x => _loadingElement.color = new Color(_loadingElement.color.r, _loadingElement.color.g, _loadingElement.color.b, x), 0, 0.2f)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    _isLoading = false;
                    Instance._loadingElement.gameObject.SetActive(false);
                    onFadeComplete?.Invoke();
                });
        }
    }
}
