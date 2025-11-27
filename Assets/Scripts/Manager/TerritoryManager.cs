using LoopLegacy.Player;
using LoopLegacy.State;
using LoopLegacy.UI.Controller;
using R3;
using UnityEngine;

namespace LoopLegacy.Manager
{
    public class TerritoryManager : MonoBehaviour
    {
        private static TerritoryManager _instance;
        public static TerritoryManager Instance => _instance;

        [SerializeField]
        private AudioClip _territoryBGM;

        [SerializeField]
        private TerritoryHUDController _territoryHUDController;

        [SerializeField]
        private ShopController _shopController;

        [SerializeField]
        private CodexController _codexController;

        //NPCs
        private GameObject _shopEquipment;
        private GameObject _codexNPC;
        private GameObject _shopUpgrade;
        private GameObject _shopRelic;
        private GameObject _bard;
        private GameObject _worker;
        private GameObject _library;

        public ReactiveProperty<bool> IsInteractionInProgress { get; private set; }

        private void Awake()
        {
            _instance = this;
            IsInteractionInProgress = new ReactiveProperty<bool>(false);
        }

        void Start()
        {
            AudioManager.Instance.PlayBGM(_territoryBGM, true);

            _shopEquipment = GameObject.Find("shop_equipment");
            _codexNPC = GameObject.Find("codex");
            _shopUpgrade = GameObject.Find("shop_upgrade");
            _shopRelic = GameObject.Find("shop_relic");
            _bard = GameObject.Find("bard");
            _worker = GameObject.Find("worker");
            _library = GameObject.Find("library");
            PersistentGameState.Instance.HouseState.TerritoryLevel.Subscribe(level =>
            {
                UpdateTerritory(level);
            });

            if (!PersistentGameState.Instance.CompletedTutorials[TutorialType.Territory])
            {
                ConfirmationController.Instance.ShowConfirmation(
                    Utils.GetUIString("start-tutorial-confirmation_territory"),
                    onConfirm: () => {
                        _territoryHUDController.HideGameStartElement();
                        ScriptManager.Instance.StartScript("tutorial_territory");
                    },
                    onClose: () => {
                        EndTerritoryTutorial();
                    },
                    confirmButtonText: Utils.GetUIString("yes"),
                    closeButtonText: Utils.GetUIString("no"));
            }
        }

        private void UpdateTerritory(int level)
        {
            if (!PersistentGameState.Instance.CompletedTutorials[TutorialType.Territory] && level == 1)
            {
                _shopController.Hide();
                ScriptManager.Instance.StartScript(
                    "tutorial_territory",
                    startIndex: 4,
                    onComplete: () => EndTerritoryTutorial());
            }

            _shopEquipment.SetActive(level > 0);
            _codexNPC.SetActive(level > 1);
            _shopRelic.SetActive(level > 2);
            _shopUpgrade.SetActive(level > 3);
            _worker.SetActive(level > 4);
            _library.SetActive(level > 5);
        }

        private void EndTerritoryTutorial()
        {
            PersistentGameState.Instance.CompleteTutorial(TutorialType.Territory);
            _territoryHUDController.ShowGameStartElement();
            PersistentGameState.Instance.SaveState();
        }

        public void StartGame()
        {
            IsInteractionInProgress.Value = true;
            if (!PersistentGameState.Instance.CompletedTutorials[TutorialType.Territory])
            {
                ConfirmationController.Instance.ShowConfirmation(
                    Utils.GetUIString("skip-territory-tutorial-confirmation"),
                    onConfirm: () => {
                        EndTerritoryTutorial();
                        IsInteractionInProgress.Value = false;
                        FadeController.LoadSceneWithMiddleScene(
                            "G-Start",
                            "PreGame",
                            FadeController.MAP_MOVE_DELAY,
                            onLoadComplete: () => PlayerMovement.Instance.SetPosition(new Vector3(-0.5f, -0.0f)));
                    },
                    onClose: () => IsInteractionInProgress.Value = false,
                    confirmButtonText: Utils.GetUIString("yes"),
                    closeButtonText: Utils.GetUIString("no"));
            }
            else
            {
                string message = Utils.GetUIString(
                    "start-game-confirmation",
                    new object[] {
                        Utils.Ordinal(PersistentGameState.Instance.CurrentLoopCount + 1),
                        $"{PersistentGameState.Instance.Gold.Value:n0}" });
                ConfirmationController.Instance.ShowConfirmation(
                    message,
                    onConfirm: () =>
                    {
                        IsInteractionInProgress.Value = false;
                        FadeController.LoadSceneWithMiddleScene(
                            "G-Start",
                            "PreGame",
                            FadeController.MAP_MOVE_DELAY,
                            onLoadComplete: () => PlayerMovement.Instance.SetPosition(new Vector3(-0.5f, -0.0f)));
                    },
                    onClose: () => IsInteractionInProgress.Value = false,
                    confirmButtonText: Utils.GetUIString("yes"),
                    closeButtonText: Utils.GetUIString("no"));
            }
        }

        public void Interact(NPCType npcType)
        {
            switch(npcType)
            {
                case NPCType.GameStart:
                    StartGame();
                    break;
                case NPCType.Codex:
                    IsInteractionInProgress.Value = true;
                    _codexController.Show(() => IsInteractionInProgress.Value = false);
                    break;
                default:
                    IsInteractionInProgress.Value = true;
                    if (npcType == NPCType.Shop_Maid ||
                        npcType == NPCType.Shop_Equipment ||
                        npcType == NPCType.Shop_Upgrade ||
                        npcType == NPCType.Shop_Relic ||
                        npcType == NPCType.Worker ||
                        npcType == NPCType.Library)
                    {
                        _territoryHUDController.HideGameStartElement();
                        _shopController.Show(npcType, () => {
                            IsInteractionInProgress.Value = false;
                            _territoryHUDController.ShowGameStartElement();
                        });
                    }
                    else
                    {
                        ScriptManager.Instance.StartScript(Utils.GetNPCCode(npcType), () => IsInteractionInProgress.Value = false, startIndex: Random.Range(0, 9));
                    }
                    break;
            }
        }
    }
}
