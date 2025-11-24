using System;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LoopLegacy.Manager
{
    public class BannerAdManager : MonoBehaviour
    {
        public static BannerAdManager Instance { get; private set; }
        private BannerView _bannerView;

#if UNITY_ANDROID
        private const string _bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
        private const string _anchoredBannerAdUnitId = "ca-app-pub-3940256099942544/9214589741";
#elif UNITY_IPHONE
        private const string _bannerAdUnitId = "ca-app-pub-3940256099942544/2934735716";
        private const string _anchoredBannerAdUnitId = "ca-app-pub-3940256099942544/2435281174";
#else
        private const string _bannerAdUnitId = "unused";
        private const string _anchoredBannerAdUnitId = "unused";
#endif

        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (_bannerView != null)
            {
                DestroyBannerAd();
            }
        }

        public void Initialize()
        {
            LoadAd();
            SceneManager.activeSceneChanged += (_, _) =>
            {
                Debug.Log("Active scene changed to: " + SceneManager.GetActiveScene().name);
                LoadAd();
            };
        }

        /// <summary>
        /// Loads the ad.
        /// </summary>
        private void LoadAd()
        {
            // Clean up the old ad before loading a new one.
            if (_bannerView != null)
            {
                DestroyBannerAd();
            }

            Debug.Log("Loading banner ad.");
            int deviceWidth = MobileAds.Utils.GetDeviceSafeWidth();
            AdSize adaptiveSize =
                AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(deviceWidth);
            Debug.Log("Adaptive size: " + adaptiveSize);

            _bannerView = new BannerView(_anchoredBannerAdUnitId, adaptiveSize, AdPosition.Bottom);
            _bannerView.LoadAd(new AdRequest());
            
            ListenToAdEvents();
        }

        /// <summary>
        /// Destroys the banner ad.
        /// </summary>
        public void DestroyBannerAd()
        {
            if (_bannerView != null)
            {
                Debug.Log("Destroying banner ad.");
                _bannerView.Destroy();
                _bannerView = null;
            }
        }

        /// <summary>
        /// Listen to events the banner may raise.
        /// </summary>
        private void ListenToAdEvents()
        {
            // Raised when an ad is loaded into the banner view.
            _bannerView.OnBannerAdLoaded += () =>
            {
                Debug.Log("Banner view loaded an ad with response : "
                    + _bannerView.GetResponseInfo());
            };
            // Raised when an ad fails to load into the banner view.
            _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                Debug.LogError("Banner view failed to load an ad with error : " + error);
            };
            // Raised when the ad is estimated to have earned money.
            _bannerView.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Banner view paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            _bannerView.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Banner view recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            _bannerView.OnAdClicked += () =>
            {
                Debug.Log("Banner view was clicked.");
            };
            // Raised when an ad opened full screen content.
            _bannerView.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Banner view full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            _bannerView.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Banner view full screen content closed.");
            };
        }
    }
}