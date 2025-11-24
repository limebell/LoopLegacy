using LoopLegacy.Manager;
using LoopLegacy.State;
using R3;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LoopLegacy.UI.Component
{
    public class StatElement : MonoBehaviour
    {
        [SerializeField] public StatType statType;
        [SerializeField] public TMP_Text _statTypeText;
        [SerializeField] public TMP_Text _statValueText;
        [SerializeField] public Button _openAddStatButton;

        public UnityEvent<StatType> onOpenAddStatButtonClicked;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _openAddStatButton.onClick.AddListener(OnOpenAddStatButtonClicked);
            if (GameManager.Instance == null)
            {
                _openAddStatButton.gameObject.SetActive(false);
            }
            else
            {
                _openAddStatButton.gameObject.SetActive(true);
                SubscribeToState();
            }
        }

        private void OnOpenAddStatButtonClicked()
        {
            onOpenAddStatButtonClicked?.Invoke(statType);
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            UpdateVisuals();
#endif
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            EditorApplication.delayCall += UpdateVisuals;
#endif
        }

        private void OnBecameVisible()
        {
            Debug.Log("OnBecameVisible: " + statType);
            UpdateValues();
        }

        private void UpdateVisuals()
        {
            _statTypeText.text = Utils.GetStatName(statType);
        }

        private void SubscribeToState()
        {
            var d = Disposable.CreateBuilder();
            GameManager.Instance.GameState.PlayerStats.Stats[(int)statType]
                .Subscribe(_ => UpdateValues())
                .AddTo(ref d);
            GameManager.Instance.GameState.OwnedRelics
                .Subscribe(_ => UpdateValues())
                .AddTo(ref d);
            d.RegisterTo(this.destroyCancellationToken);
        }

        public void UpdateValues()
        {
            if (GameManager.Instance == null)
            {
                _statValueText.text = $"{Mathf.RoundToInt(PersistentGameState.Instance.GetBaseStat(statType) * LibraryManager.GetStatMultiplier(PersistentGameState.Instance.AccumulatedLevel)):n0}";
                return;
            }
            
            var statBoosts = GameManager.Instance.RelicManager.GetStatBoost();
            int boost = Mathf.RoundToInt(statBoosts[statType] * LibraryManager.GetStatMultiplier(PersistentGameState.Instance.AccumulatedLevel));

            var baseStat = Mathf.RoundToInt(GameManager.Instance.GameState.PlayerStats.Stats[(int)statType].Value * LibraryManager.GetStatMultiplier(PersistentGameState.Instance.AccumulatedLevel));
            string statText = $"{baseStat:n0}";
            if (boost > 0) statText += $" (+ {boost:n0})";
            _statValueText.text = statText;
        }
    }
}
