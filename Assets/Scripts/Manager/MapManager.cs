using LoopLegacy.Player;
using LoopLegacy.State;
using LoopLegacy.UI.Controller;
using UnityEngine;

namespace LoopLegacy.Manager
{
    public class MapManager : MonoBehaviour
    {
        [SerializeField] private Vector2 _playerPosition;
        [SerializeField] private AudioClip _mapBGM;
        [SerializeField] private bool _showFog = false;

        private static bool _forceMove = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            #if UNITY_EDITOR
                if (GameObject.Find("GameEssentials") == null)
                {
                    Debug.LogError("TestMapManager: GameEssentials not found, loading PreGame");
                    FadeController.LoadSceneWithMiddleScene(
                        gameObject.scene.name,
                        "PreGame",
                        FadeController.MAP_MOVE_DELAY,
                        onLoadComplete: () => _forceMove = true);
                }
            #endif

            AudioManager.Instance?.PlayBGM(_mapBGM, true);
            Debug.Log("[MapManager] Start: gameObject.scene.name: " + gameObject.scene.name);
            if (gameObject.scene.name.StartsWith("G-"))
            {
                GameManager.Instance.GameState.CurrentMapCode = gameObject.scene.name[2..];
                Debug.Log("[MapManager] Start: GameManager.Instance.GameState.CurrentMapCode: " + GameManager.Instance.GameState.CurrentMapCode);
            }

            if (GameManager.Instance.GameState.CurrentMapCode != "Tutorial" &&
                !PersistentGameState.Instance.CompletedTutorials[TutorialType.GameStart])
            {
                Debug.Log("[MapManager] Start: ShowConfirmation");
                string startTutorialConfirmation = Utils.GetUIString("start-tutorial-confirmation_basic");
                string yesText = Utils.GetUIString("yes");
                string noText = Utils.GetUIString("no");
                ConfirmationController.Instance.ShowConfirmation(
                    message: startTutorialConfirmation,
                    onConfirm: () => FadeController.LoadScene("G-Tutorial", FadeController.MAP_MOVE_DELAY),
                    onClose: () => PersistentGameState.Instance.CompleteTutorial(TutorialType.GameStart),
                    confirmButtonText: yesText,
                    closeButtonText: noText);
            }

            if (CameraFollow.Instance != null)
            {
                CameraFollow.Instance.SetFogActive(_showFog);
            }

            CheckShortcut();
        }

#if UNITY_EDITOR
        void Update()
        {
            if (_forceMove && CameraFollow.Instance != null && PlayerMovement.Instance != null)
            {
                CameraFollow.Instance.SetPosition(_playerPosition);
                PlayerMovement.Instance.SetPosition(_playerPosition);
                _forceMove = false;
            }
        }
#endif

        private void CheckShortcut()
        {
            if (PersistentGameState.Instance.HouseState.GetShortcut(Shortcut.Desert))
            {
                GameObject.Find("Shortcut_Desert-cover")?.SetActive(false);
                GameObject.Find("Shortcut_Desert")?.SetActive(true);
                GameObject.Find("Region-desert-0")?.SetActive(false);
                GameObject.Find("Region-desert-shortcut")?.SetActive(true);
                GameObject.Find("Region-pyramid-shortcut")?.SetActive(true);
            }
            else
            {
                GameObject.Find("Shortcut_Desert-cover")?.SetActive(true);
                GameObject.Find("Shortcut_Desert")?.SetActive(false);
                GameObject.Find("Region-desert-0")?.SetActive(true);
                GameObject.Find("Region-desert-shortcut")?.SetActive(false);
                GameObject.Find("Region-pyramid-shortcut")?.SetActive(false);
            }

            if (PersistentGameState.Instance.HouseState.GetShortcut(Shortcut.DeepForest))
            {
                GameObject.Find("Shortcut_DeepForest-cover")?.SetActive(false);
            }
            else
            {
                GameObject.Find("Shortcut_DeepForest-cover")?.SetActive(true);
            }

            if (PersistentGameState.Instance.HouseState.GetShortcut(Shortcut.Castle))
            {
                GameObject.Find("Shortcut_Castle-cover_0")?.SetActive(false);
                GameObject.Find("Shortcut_Castle-cover_1")?.SetActive(false);
                GameObject.Find("Shortcut_Castle")?.SetActive(true);
            }
            else
            {
                GameObject.Find("Shortcut_Castle-cover_0")?.SetActive(true);
                GameObject.Find("Shortcut_Castle-cover_1")?.SetActive(true);
                GameObject.Find("Shortcut_Castle")?.SetActive(false);
            }
        }
    }
}
