using GoogleMobileAds;
using GoogleMobileAds.Api;
using LoopLegacy.Manager;
using LoopLegacy.State;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

namespace LoopLegacy
{
    public class InitialManager : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            InitializeData();
            InitializeLocalization();
            InitializeQuality();
#if UNITY_ANDROID || UNITY_IOS
            InitializeGoogleAdMob();
            // Disable Banner Ad for now, first resolve canvas view issue
            //BannerAdManager.Instance?.Initialize();
#endif

            SceneManager.LoadScene("Title");
        }

        private void InitializeData()
        {
            try
            {
                // 테이블 데이터 로드
                Debug.Log("Loading tables...");
                TableManager.LoadAllTables();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DataLoader] 데이터 초기화 중 오류 발생: {e.Message}");
                throw;
            }
        }

        private async void InitializeLocalization()
        {
            await LocalizationSettings.InitializationOperation.Task;
            LocalizationSettings.SelectedLocale =
                LocalizationSettings.AvailableLocales.GetLocale(OptionState.Instance.Language.Value);
        }

        private void InitializeQuality()
        {
            QualitySettings.SetQualityLevel(OptionState.Instance.Quality.Value + 1);
            Application.targetFrameRate = OptionState.Instance.FrameRate.Value switch
            {
                0 => 30,
                1 => 48,
                2 => 60,
                _ => 30
            };
        }

        private void InitializeGoogleAdMob()
        {
            MobileAds.Initialize((InitializationStatus initstatus) =>
            {
                if (initstatus == null)
                {
                    Debug.LogError("Google Mobile Ads initialization failed.");
                    return;
                }

                Debug.Log("Google Mobile Ads initialization complete.");

                // Google Mobile Ads events are raised off the Unity Main thread. If you need to
                // access UnityEngine objects after initialization,
                // use MobileAdsEventExecutor.ExecuteInUpdate(). For more information, see:
                // https://developers.google.com/admob/unity/global-settings#raise_ad_events_on_the_unity_main_thread
            });
        }
    }
}
