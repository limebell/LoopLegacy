using GoogleMobileAds;
using GoogleMobileAds.Api;
using R3;
using System;
using UnityEngine;

namespace LoopLegacy
{
    public class RewardedAdManager : MonoBehaviour
    {
        public static RewardedAdManager Instance { get; private set; }

        public ReactiveProperty<bool> IsLoaded { get; private set; }

        private RewardedAd _rewardedAd;

        // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
        private const string _bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
        private const string _anchoredBannerAdUnitId = "ca-app-pub-3940256099942544/9214589741";
        private const string _rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
        private const string _bannerAdUnitId = "ca-app-pub-3940256099942544/2934735716";
        private const string _anchoredBannerAdUnitId = "ca-app-pub-3940256099942544/2435281174";
        private const string _rewardedAdUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
        private const string _bannerAdUnitId = "unused";
        private const string _anchoredBannerAdUnitId = "unused";
        private const string _rewardedAdUnitId = "unused";
#endif

        void Awake()
        {
            Instance = this;
            IsLoaded = new ReactiveProperty<bool>(false);
        }

        void Start()
        {
#if UNITY_ANDROID || UNITY_IOS
            LoadAd(() => { IsLoaded.Value = true; });
#else
            IsLoaded.Value = true;
#endif
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (_rewardedAd != null)
            {
                DestroyRewardedAd();
            }
        }

        /// <summary>
        /// Loads the ad.
        /// </summary>
        public void LoadAd(Action onLoaded)
        {
            // Clean up the old ad before loading a new one.
            if (_rewardedAd != null)
            {
                DestroyRewardedAd();
            }

            Debug.Log("Loading rewarded ad.");
            // Create our request used to load the ad.

            // Send the request to load the ad.
            RewardedAd.Load(_rewardedAdUnitId, new AdRequest(), (RewardedAd ad, LoadAdError error) =>
            {
                // If the operation failed with a reason.
                if (error != null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad with error : " + error);
                    return;
                }
                // If the operation failed for unknown reasons.
                // This is an unexpected error, please report this bug if it happens.
                if (ad == null)
                {
                    Debug.LogError("Unexpected error: Rewarded load event fired with null ad and null error.");
                    return;
                }

                // The operation completed successfully.
                Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());
                _rewardedAd = ad;

                // Register to ad events to extend functionality.
                RegisterRewardedAdEventHandlers(ad);
                onLoaded?.Invoke();
            });
        }

        /// <summary>
        /// Shows the ad.
        /// </summary>
        public void ShowRewardedAd(Action<Reward> onRewarded)
        {
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                Debug.Log("Showing rewarded ad.");
                _rewardedAd.Show(onRewarded);
            }
            else
            {
                Debug.LogError("Rewarded ad is not ready yet.");
            }
        }

        /// <summary>
        /// Destroys the rewarded ad.
        /// </summary>
        public void DestroyRewardedAd()
        {
            if (_rewardedAd != null)
            {
                Debug.Log("Destroying rewarded ad.");
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }
        }

        /// <summary>
        /// Logs the ResponseInfo.
        /// </summary>
        public void LogResponseInfo()
        {
            if (_rewardedAd != null)
            {
                var responseInfo = _rewardedAd.GetResponseInfo();
                Debug.Log(responseInfo);
            }
        }

        private void RegisterRewardedAdEventHandlers(RewardedAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Rewarded ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Rewarded ad was clicked.");
            };
            // Raised when the ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Rewarded ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Rewarded ad full screen content closed.");
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Rewarded ad failed to open full screen content with error : "
                    + error);
            };
        }
    }
}
