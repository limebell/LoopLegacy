using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LoopLegacy.Manager
{
    public class BannerAdManager : MonoBehaviour
    {
        public static BannerAdManager Instance { get; private set; }
        private BannerView _bannerView;
        private AdSize _currentAdSize;
        private bool _pendingSceneChange;
        private Dictionary<RectTransform, float> _originalOffsetMaxY = new Dictionary<RectTransform, float>();
        private Dictionary<RectTransform, CoverOriginalState> _originalCoverStates = new Dictionary<RectTransform, CoverOriginalState>();
        private Dictionary<Camera, Rect> _originalCameraRects = new Dictionary<Camera, Rect>();

        private struct CoverOriginalState
        {
            public Vector2 anchorMin;
            public Vector2 anchorMax;
            public Vector2 sizeDelta;
            public Vector2 anchoredPosition;
        }

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

        void Update()
        {
            // 씬 변경 후 메인 스레드에서 조정 수행
            if (_pendingSceneChange)
            {
                _pendingSceneChange = false;
            
                AdjustCanvasesForBanner();
                AdjustCameraViewports();
            }
        }

        public void Initialize()
        {
            SceneManager.activeSceneChanged += (activeScene, nextScene) =>
            {
                if (nextScene.name == "Title" || nextScene.name == "PreGame")
                {
                    RestoreCanvases();
                    return;
                }

                // 씬 변경 시 다음 프레임에서 LoadAd 호출 (새 씬의 Canvas를 안전하게 찾기 위해)
                LoadAd();
                _pendingSceneChange = true;
            };
        }

        /// <summary>
        /// Loads the ad.
        /// </summary>
        private void LoadAd()
        {
            // 광고 제거 구매 여부 확인
            if (IAPManager.Instance != null && IAPManager.Instance.IsAdsRemoved)
            {
                Debug.Log("[BannerAdManager] 광고가 제거되어 배너를 로드하지 않습니다.");
                return;
            }

            // Clean up the old ad before loading a new one.
            if (_bannerView != null)
            {
                DestroyBannerAd();
            }

            Debug.Log("Loading banner ad.");
            int deviceWidth = MobileAds.Utils.GetDeviceSafeWidth();
            AdSize adaptiveSize =
                AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(deviceWidth);
            
            // AdSize 저장 (GetWidthInPixels가 0을 반환할 수 있으므로)
            _currentAdSize = adaptiveSize;

            _bannerView = new BannerView(_anchoredBannerAdUnitId, adaptiveSize, AdPosition.Top);
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
            
            // 배너 제거 시 Canvas 원복
            RestoreCanvases();
        }

        /// <summary>
        /// 현재 씬의 모든 Canvas 상단에 배너 높이만큼 여백을 추가합니다.
        /// 각 Canvas의 너비와 배너의 aspect ratio를 이용해서 해당 Canvas에서의 배너 높이를 계산합니다.
        /// AdSize의 Width와 Height는 dp 단위이지만, aspect ratio는 단위와 무관하므로 직접 사용 가능합니다.
        /// </summary>
        private void AdjustCanvasesForBanner()
        {
            if (_bannerView == null || _currentAdSize == null) return;

            if (!TryGetTopInsetRatio(out float topInsetRatio))
                return;
            
            // 배너 높이가 유효할 때만 기존 조정된 Canvas 원복
            RestoreCanvases();
            
            // 현재 씬의 모든 Canvas 찾기
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            
            foreach (Canvas canvas in allCanvases)
            {
                // 루트 Canvas만 조정 (중첩된 Canvas 제외)
                if (canvas.transform.parent != null && canvas.transform.parent.GetComponent<Canvas>() != null)
                    continue;
                
                // 배너 광고 Canvas는 제외
                if (canvas.name.Contains("ADAPTIVE"))
                    continue;
                
                // Canvas의 렌더 모드에 따라 처리
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && 
                    canvas.renderMode != RenderMode.ScreenSpaceCamera)
                    continue;
                
                // Canvas의 높이를 가져옴 (논리적 단위)
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                float canvasHeight = canvasRect.rect.height;

                // 상단 인셋(상태바 + 배너 하단까지)을 Canvas 논리 단위로 변환
                float offsetFromTop = canvasHeight * topInsetRatio;
                
                // Canvas의 모든 직접 자식들의 하단 오프셋 조정
                foreach (Transform child in canvas.transform)
                {
                    RectTransform childRect = child.GetComponent<RectTransform>();
                    if (childRect == null) continue;
                    
                    // "Cover" 오브젝트는 배너 영역을 덮도록 특별 처리
                    if (child.name == "Cover")
                    {
                        AdjustCover(childRect, offsetFromTop);
                        continue;
                    }
                    
                    // 원래 offsetMax.y 저장
                    if (!_originalOffsetMaxY.ContainsKey(childRect))
                    {
                        _originalOffsetMaxY[childRect] = childRect.offsetMax.y;
                    }
                    
                    // offsetMax.y에서 배너 하단 위치만큼 빼기 (배너 하단까지 여백 생성)
                    Vector2 newOffsetMax = childRect.offsetMax;
                    newOffsetMax.y = _originalOffsetMaxY[childRect] - offsetFromTop;
                    childRect.offsetMax = newOffsetMax;
                }
            }
        }

        /// <summary>
        /// Cover 오브젝트를 배너 영역을 덮도록 조정합니다.
        /// </summary>
        private void AdjustCover(RectTransform coverRect, float coverHeight)
        {
            // 원래 상태 저장
            if (!_originalCoverStates.ContainsKey(coverRect))
            {
                _originalCoverStates[coverRect] = new CoverOriginalState
                {
                    anchorMin = coverRect.anchorMin,
                    anchorMax = coverRect.anchorMax,
                    sizeDelta = coverRect.sizeDelta,
                    anchoredPosition = coverRect.anchoredPosition
                };
            }
            
            // 상단 stretch로 설정 (좌우로 늘어나고, 상단에 고정)
            coverRect.anchorMin = new Vector2(0, 1);
            coverRect.anchorMax = new Vector2(1, 1);
            coverRect.pivot = new Vector2(0.5f, 1);
            
            // 크기: 좌우는 0 (stretch), 높이는 배너 높이만큼
            coverRect.sizeDelta = new Vector2(0, coverHeight);
            
            // 위치: 상단에 붙음
            coverRect.anchoredPosition = Vector2.zero;
        }

        /// <summary>
        /// 모든 카메라의 뷰포트를 배너 높이만큼 조정합니다.
        /// 배너의 물리적 픽셀 높이를 Screen.height와 비교해서 비율을 계산합니다.
        /// GetHeightInPixels()가 0이면 AdSize의 aspect ratio를 사용해서 추정합니다.
        /// </summary>
        private void AdjustCameraViewports()
        {
            if (_bannerView == null || _currentAdSize == null) return;

            if (!TryGetTopInsetRatio(out float topInsetRatio))
                return;
            
            // 모든 카메라 찾기
            Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            
            foreach (Camera cam in allCameras)
            {
                // 원래 rect 저장
                if (!_originalCameraRects.ContainsKey(cam))
                {
                    _originalCameraRects[cam] = cam.rect;
                }
                
                Rect originalRect = _originalCameraRects[cam];
                
                // 뷰포트 상단을 '상태바 + 배너 하단까지' 만큼 줄임 (y는 유지, height만 줄임)
                Rect newRect = new Rect(
                    originalRect.x,
                    originalRect.y,
                    originalRect.width,
                    Mathf.Max(0f, originalRect.height - topInsetRatio)
                );
                
                cam.rect = newRect;
            }
        }

        private bool TryGetTopInsetRatio(out float topInsetRatio)
        {
            topInsetRatio = 0f;

            if (_bannerView == null || _currentAdSize == null)
                return false;

            if (Screen.height <= 0)
                return false;

            if (!TryGetBannerHeightInPixels(out float bannerHeightInPixels))
                return false;

            Rect safeArea = Screen.safeArea;
            float statusBarHeightInPixels = Screen.height - safeArea.yMax;

            // 화면 상단 기준: 배너 하단까지의 거리 = 상태바 높이 + 배너 높이
            float bannerBottomFromTopInPixels = statusBarHeightInPixels + bannerHeightInPixels;

            topInsetRatio = Mathf.Clamp01(bannerBottomFromTopInPixels / Screen.height);
            return topInsetRatio > 0f;
        }

        private bool TryGetBannerHeightInPixels(out float bannerHeightInPixels)
        {
            bannerHeightInPixels = 0f;

            if (_bannerView == null)
                return false;

            bannerHeightInPixels = _bannerView.GetHeightInPixels();
            if (bannerHeightInPixels > 0f)
                return true;

            // 네이티브 픽셀 값이 0이면 aspect ratio 기반으로 추정
            if (_currentAdSize == null)
                return false;

            float ratio = GetBannerRatio();
            if (ratio <= 0f)
                return false;

            bannerHeightInPixels = Screen.width * ratio;
            return bannerHeightInPixels > 0f;
        }

        /// <summary>
        /// 배너의 aspect ratio를 계산합니다.
        /// 네이티브 뷰는 물리적 픽셀 단위를 사용하므로, GetHeightInPixels()와 Screen.width를 사용합니다.
        /// </summary>
        private float GetBannerRatio()
        {
            // 네이티브 뷰는 물리적 픽셀 단위를 사용
            // GetHeightInPixels()는 물리적 픽셀 단위의 배너 높이를 반환
            // 배너는 스크린 폭에 맞춰지므로, 배너 너비 ≈ Screen.width (물리적 픽셀)
            
            float bannerHeightInPixels = _bannerView.GetHeightInPixels();
            float bannerWidthInPixels = _bannerView.GetWidthInPixels();
            
            // GetHeightInPixels()가 유효하면 사용
            if (bannerHeightInPixels > 0)
            {
                // GetWidthInPixels()가 유효하면 그것을 사용, 아니면 Screen.width 사용
                float width = bannerWidthInPixels > 0 ? bannerWidthInPixels : Screen.width;
                float ratio = bannerHeightInPixels / width;
                return ratio;
            }
            
            // GetHeightInPixels()가 0이면 AdSize의 dp 값으로 추정
            // AdSize.Width는 dp 단위이지만, aspect ratio는 단위와 무관
            if (_currentAdSize.Width > 0 && _currentAdSize.Height > 0)
            {
                float ratio = (float)_currentAdSize.Height / (float)_currentAdSize.Width;
                return ratio;
            }
            else if (_currentAdSize.Width > 0)
            {
                // Adaptive banner: 일반적인 높이(50dp) 사용
                float ratio = 50f / (float)_currentAdSize.Width;
                return ratio;
            }
            
            return 0f;
        }
        /// <summary>
        /// 조정된 Canvas들을 원래 상태로 복원합니다.
        /// </summary>
        private void RestoreCanvases()
        {
            // Canvas 자식들 복원
            foreach (var kvp in _originalOffsetMaxY)
            {
                RectTransform rectTransform = kvp.Key;
                float originalY = kvp.Value;
                
                if (rectTransform == null) continue;
                
                Vector2 offsetMax = rectTransform.offsetMax;
                offsetMax.y = originalY;
                rectTransform.offsetMax = offsetMax;
            }
            _originalOffsetMaxY.Clear();
            
            // Cover 오브젝트 복원
            foreach (var kvp in _originalCoverStates)
            {
                RectTransform coverRect = kvp.Key;
                CoverOriginalState state = kvp.Value;
                
                if (coverRect == null) continue;
                
                coverRect.anchorMin = state.anchorMin;
                coverRect.anchorMax = state.anchorMax;
                coverRect.sizeDelta = state.sizeDelta;
                coverRect.anchoredPosition = state.anchoredPosition;
            }
            _originalCoverStates.Clear();
            
            // 카메라 뷰포트 복원
            foreach (var kvp in _originalCameraRects)
            {
                Camera cam = kvp.Key;
                Rect originalRect = kvp.Value;
                
                if (cam == null) continue;
                
                cam.rect = originalRect;
            }
            _originalCameraRects.Clear();
        }

        /// <summary>
        /// Listen to events the banner may raise.
        /// </summary>
        private void ListenToAdEvents()
        {
            // Raised when an ad is loaded into the banner view.
            _bannerView.OnBannerAdLoaded += () =>
            {
                _pendingSceneChange = true;
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
                // 배너가 완전히 렌더링된 후 크기 정보가 사용 가능할 수 있음
                _pendingSceneChange = true;
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