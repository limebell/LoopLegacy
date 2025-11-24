using LoopLegacy.UI.Component;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using LoopLegacy.UI.Helper;

namespace LoopLegacy.UI.Controller
{
    public class ShopController : MonoBehaviour
    {
        [SerializeField] private Image _npcImage;
        [SerializeField] private TMP_Text _npcName;
        [SerializeField] private TMP_Text _dialogueText;
        [SerializeField] private GameObject _priceObject;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _upButton;
        [SerializeField] private Button _downButton;
        [SerializeField] private TMP_Text _pageText;
        [SerializeField] public Button[] CategoryButtons;
        [SerializeField] private ShopElement[] _shopElements;
        [SerializeField] private InitialRelicSelectController _initialRelicSelectController;
        [SerializeField] private LibraryController _libraryController;
        private ShopHelper _shopHelper;
        private NPCType _currentNpcType;
        protected int _currentCategory;
        protected int _currentPageIndex;
        protected int _selectedIndex;
        protected List<object> _shopList = new List<object>();
        private Action _onClose;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _closeButton.onClick.AddListener(Hide);
            _buyButton.onClick.AddListener(OnBuyButtonClicked);
            _upButton.onClick.AddListener(OnUpButtonClicked);
            _downButton.onClick.AddListener(OnDownButtonClicked);
            for (int i = 0; i < CategoryButtons.Length; i++)
            {
                int index = i;
                CategoryButtons[i].onClick.AddListener(() => SetCategory(index));
            }
        }

        public void Show(NPCType npcType, Action onClose)
        {
            HideInitialRelicSelect();
            HideLibrary();
            gameObject.SetActive(true);
            _onClose = onClose;
            _npcName.text = Utils.GetNPCName(npcType);
            _currentNpcType = npcType;
            _npcImage.sprite = Addressables.LoadAssetAsync<Sprite>("Images/NPC/" + Utils.GetNPCCode(npcType)).WaitForCompletion();
            _shopHelper = npcType switch
            {
                NPCType.Shop_Maid => new MaidShopHelper(this),
                NPCType.Shop_Equipment => new EquipmentShopHelper(this),
                NPCType.Shop_Upgrade => new UpgradeShopHelper(this),
                NPCType.Shop_Relic => new RelicShopHelper(this),
                NPCType.Worker => new WorkerShopHelper(this),
                NPCType.Library => new LibraryHelper(this),
                _ => throw new NotImplementedException(),
            };
            _shopHelper.Initialize();
            SetCategory(0);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _onClose?.Invoke();
            _onClose = null;
        }

        private void SetCategory(int category)
        {
            for (int i = 0; i < CategoryButtons.Length; i++)
            {
                CategoryButtons[i].interactable = i != category;
            }
            
            _currentCategory = category;
            _shopList.Clear();
            _shopList.AddRange(_shopHelper.Populate(category));
            _currentPageIndex = 0;
            ClampPageIndex();
            Select(-1);
        }

        private void OnBuyButtonClicked()
        {
            _shopHelper.Buy(_currentCategory, _shopList[_selectedIndex], () => Select(_selectedIndex));
        }

        private void OnUpButtonClicked()
        {
            _currentPageIndex--;
            ClampPageIndex();
            UpdateShopList();
        }

        private void OnDownButtonClicked()
        {
            _currentPageIndex++;
            ClampPageIndex();
            UpdateShopList();
        }

        private void ClampPageIndex()
        {
            int maxPageIndex = (_shopList.Count - 1) / _shopElements.Length;
            _currentPageIndex = Mathf.Max(0, Mathf.Min(_currentPageIndex, maxPageIndex));
            _pageText.text = maxPageIndex > 0 ? $"{_currentPageIndex + 1}/{maxPageIndex + 1}" : "";
        }

        private void Select(int index)
        {
            _selectedIndex = index;
            if (index == -1)
            {
                _dialogueText.text = Utils.GetNPCDescription(_currentNpcType);
                _priceText.text = "";
                _priceObject.SetActive(false);
            }
            else
            {
                _priceObject.SetActive(true);
                _shopHelper.Select(_currentCategory, _shopList[index]);
            }

            UpdateShopList();
        }

        public void UpdateDialogueText(string dialogueText, string priceText)
        {
            _dialogueText.text = dialogueText;
            _priceText.text = priceText;
        }

        private void UpdateShopList()
        {
            _upButton.gameObject.SetActive(_currentPageIndex > 0);
            _downButton.gameObject.SetActive(_currentPageIndex < (_shopList.Count - 1) / _shopElements.Length);
            for (int i = _shopElements.Length * _currentPageIndex; i < _shopElements.Length * (_currentPageIndex + 1); i++)
            {
                int index = i;
                var element = _shopElements[i % _shopElements.Length];
                element.gameObject.SetActive(i < _shopList.Count);
                if (i >= _shopList.Count) continue;
                _shopHelper.SetShopElement(element, _currentCategory, _shopList[i]);
                element.SetCallback(() => Select(index));
                element.ToggleSelect(i == _selectedIndex);
            }
        }

        public void ShowInitialRelicSelect()
        {
            _initialRelicSelectController.gameObject.SetActive(true);
        }

        public void HideInitialRelicSelect()
        {
            _initialRelicSelectController.gameObject.SetActive(false);
        }

        public void ShowLibrary()
        {
            _libraryController.Show();
        }

        public void HideLibrary()
        {
            _libraryController.Hide();
        }
    }
}
