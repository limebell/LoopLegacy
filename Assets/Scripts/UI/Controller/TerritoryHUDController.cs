using LoopLegacy.Manager;
using LoopLegacy.Player;
using LoopLegacy.State;
using LoopLegacy.UI.Component;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoopLegacy.UI.Controller
{
    public class TerritoryHUDController : MonoBehaviour
    {

        [Header("Controllers")]
        [SerializeField] private MenuController _menuController;
        [SerializeField] private StatsController _statsController;

        [Header("Controller")]
        [SerializeField] private MovementController _movementController;

        [Header("Buttons")]
        [SerializeField] private Button _menuButton;
        [SerializeField] private Button _statsButton;
        [SerializeField] private Button _gameStartButton;
        [SerializeField] private GameObject _gameStartElement;

        [Space(10f)]
        [SerializeField] private TMP_Text _goldLabel;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _statsButton.onClick.AddListener(OnStatsButtonClicked);
            _menuButton.onClick.AddListener(OnMenuButtonClicked);
            _gameStartButton.onClick.AddListener(OnGameStartButtonClicked);

            SubscribeToPlayerStats();
            SubscribeToControllerInput();
        }

        private void SubscribeToPlayerStats()
        {
            var d = Disposable.CreateBuilder();
            string goldString = Utils.GetUIString("owned-gold");
            PersistentGameState.Instance.Gold
                .Subscribe(gold => _goldLabel.text = $"{gold:N0}")
                .AddTo(ref d);
            d.RegisterTo(this.destroyCancellationToken);
        }

        private void SubscribeToControllerInput()
        {
            var d = Disposable.CreateBuilder();
            _movementController.Direction.Subscribe(direction => {
                        PlayerMovement.Instance.SetVector2Input(direction);
                })
                .AddTo(ref d);
            d.RegisterTo(this.destroyCancellationToken);
        }

        private void OnGameStartButtonClicked()
        {
            TerritoryManager.Instance.StartGame();
        }

        private void OnMenuButtonClicked()
        {
            _menuController.Show();
        }

        private void OnStatsButtonClicked()
        {
            _statsController.Show(null);
        }

        // Used for tutorial
        public void HideGameStartElement()
        {
            _gameStartElement.SetActive(false);
        }

        public void ShowGameStartElement()
        {
            _gameStartElement.SetActive(true);
        }
    }
}
