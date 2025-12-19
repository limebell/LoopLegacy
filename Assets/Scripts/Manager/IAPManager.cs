using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;

namespace LoopLegacy.Manager
{
    public class IAPManager : MonoBehaviour
    {
        public static IAPManager Instance { get; private set; }

        private StoreController _storeController;
        private bool _isInitialized = false;
        private List<Product> _products = new List<Product>();

        // 제품 ID (Google Play Console / App Store Connect에서 설정한 ID와 동일해야 함)
        public const string PRODUCT_REMOVE_ADS = "remove_ads";

        // 광고 제거 상태
        private const string PREFS_ADS_REMOVED = "AdsRemoved";
        public bool IsAdsRemoved => PlayerPrefs.GetInt(PREFS_ADS_REMOVED, 0) == 1;

        // 초기화 완료 여부
        public bool IsInitialized => _isInitialized;

        // 이벤트
        public event Action OnPurchaseSuccessEvent;
        public event Action<string> OnPurchaseFailedEvent;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task Initialize()
        {
            if (_storeController != null) return;

            Debug.Log("[IAPManager] Unity IAP 초기화 시작...");

            try
            {
                // StoreController 인스턴스 가져오기
                _storeController = UnityIAPServices.StoreController();

                // 이벤트 핸들러 등록
                _storeController.OnProductsFetched += OnProductsFetched;
                _storeController.OnProductsFetchFailed += OnProductsFetchFailed;
                _storeController.OnPurchasesFetched += OnPurchasesFetched;
                _storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
                _storeController.OnPurchasePending += OnPurchasePending;
                _storeController.OnPurchaseFailed += OnPurchaseFailed;
                _storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
                _storeController.OnStoreDisconnected += OnStoreDisconnected;

                // 스토어 연결
                await _storeController.Connect();

                // 제품 정의
                var productDefinitions = new List<ProductDefinition>
                {
                    new ProductDefinition(PRODUCT_REMOVE_ADS, ProductType.NonConsumable)
                };

                // 제품 정보 가져오기
                _storeController.FetchProducts(productDefinitions);

                Debug.Log("[IAPManager] Unity IAP 연결 성공");
            }
            catch (Exception e)
            {
                Debug.LogError($"[IAPManager] Unity IAP 초기화 실패: {e.Message}");
            }
        }

        private void OnDestroy()
        {
            if (_storeController != null)
            {
                _storeController.OnProductsFetched -= OnProductsFetched;
                _storeController.OnProductsFetchFailed -= OnProductsFetchFailed;
                _storeController.OnPurchasesFetched -= OnPurchasesFetched;
                _storeController.OnPurchasesFetchFailed -= OnPurchasesFetchFailed;
                _storeController.OnPurchasePending -= OnPurchasePending;
                _storeController.OnPurchaseFailed -= OnPurchaseFailed;
                _storeController.OnPurchaseConfirmed -= OnPurchaseConfirmed;
                _storeController.OnStoreDisconnected -= OnStoreDisconnected;
            }
        }

        #region Event Handlers

        private void OnProductsFetched(List<Product> products)
        {
            Debug.Log($"[IAPManager] 제품 정보 가져오기 완료: {products.Count}개");
            _isInitialized = true;
            _products = products;

            foreach (var product in products)
            {
                Debug.Log($"[IAPManager] 제품: {product.definition.id}, 가격: {product.metadata.localizedPriceString}");
            }

            // 기존 구매 내역 가져오기
            _storeController.FetchPurchases();
        }

        private void OnProductsFetchFailed(ProductFetchFailed fetchFailed)
        {
            Debug.LogError($"[IAPManager] 제품 정보 가져오기 실패: {fetchFailed.FailureReason}");
            _isInitialized = false;
        }

        private void OnPurchasesFetched(Orders orders)
        {
            foreach (var order in orders.ConfirmedOrders)
            {
                ProcessPurchase(order);
            }
        }

        private void ProcessPurchase(Order order)
        {
            var productId = order.Info.PurchasedProductInfo[0].productId;
            // Handle other product types as needed
            Debug.Log($"[IAPManager] 복원된 구매: {productId}");

            if (productId == PRODUCT_REMOVE_ADS)
            {
                SetAdsRemoved(true);
                OnPurchaseSuccessEvent?.Invoke();
            }
        }

        private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failureDescription)
        {
            Debug.LogError($"[IAPManager] 구매 내역 가져오기 실패: {failureDescription.FailureReason}");
        }

        private void OnPurchasePending(PendingOrder pendingOrder)
        {
            ProcessPurchase(pendingOrder);
        }

        private void OnPurchaseConfirmed(Order order)
        {
            Debug.Log($"[IAPManager] 구매 확인됨: {order.Info.PurchasedProductInfo[0].productId}");
        }

        private void OnPurchaseFailed(FailedOrder failedOrder)
        {
            Debug.LogError($"[IAPManager] 구매 실패: {failedOrder.Info.PurchasedProductInfo[0].productId}, 이유: {failedOrder.FailureReason}");
            OnPurchaseFailedEvent?.Invoke(failedOrder.FailureReason.ToString());
        }

        private void OnStoreDisconnected(StoreConnectionFailureDescription failureDescription)
        {
            Debug.LogError($"[IAPManager] 스토어 연결 해제: {failureDescription.Message}");
            _isInitialized = false;
        }

        #endregion

        /// <summary>
        /// 광고 제거 상품 구매 시작
        /// </summary>
        public void PurchaseRemoveAds()
        {
            if (_storeController == null || !_isInitialized)
            {
                Debug.LogError("[IAPManager] 스토어가 초기화되지 않았습니다.");
                OnPurchaseFailedEvent?.Invoke("store_not_initialized");
                return;
            }

            Debug.Log($"[IAPManager] 구매 시작: {PRODUCT_REMOVE_ADS}");
            _storeController.PurchaseProduct(PRODUCT_REMOVE_ADS);
        }

        /// <summary>
        /// 이전 구매 복원
        /// </summary>
        public void RestorePurchases(Action<bool> onComplete = null)
        {
            if (_storeController == null || !_isInitialized)
            {
                Debug.LogError("[IAPManager] 스토어가 초기화되지 않았습니다.");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log("[IAPManager] 구매 복원 시작...");
            _storeController.FetchPurchases();
            onComplete?.Invoke(true);
        }

        /// <summary>
        /// 광고 제거 상태 설정
        /// </summary>
        private void SetAdsRemoved(bool removed)
        {
            PlayerPrefs.SetInt(PREFS_ADS_REMOVED, removed ? 1 : 0);
            PlayerPrefs.Save();

            // 배너 광고 즉시 제거
            if (removed && BannerAdManager.Instance != null)
            {
                BannerAdManager.Instance.DestroyBannerAd();
            }
        }

        /// <summary>
        /// 광고 제거 상품의 가격 문자열 반환
        /// </summary>
        public string GetRemoveAdsPrice()
        {
            if (_storeController == null || !_isInitialized) return "";

            foreach (var product in _products)
            {
                if (product.definition.id == PRODUCT_REMOVE_ADS)
                {
                    return product.metadata.localizedPriceString;
                }
            }
            return "";
        }
    }
}
