using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.Text.RegularExpressions;
#endif
#if MODULE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Watermelon
{
#if MODULE_ADMOB
    public class AdMobHandler : AdvertisingHandler
    {
        private const int RETRY_ATTEMPT_DEFAULT_VALUE = 1;

        private int interstitialRetryAttempt = RETRY_ATTEMPT_DEFAULT_VALUE;
        private int rewardedRetryAttempt = RETRY_ATTEMPT_DEFAULT_VALUE;

        private BannerView bannerView;
        private InterstitialAd interstitial;
        private RewardedAd rewardBasedVideo;
        private bool isRewarded;

#if UNITY_EDITOR
        private readonly string ADMOB_GAMEOBJECT_NAME_REGEX = "[0-9]{3,4}x[0-9]{3,4}\\(Clone\\)";
        private Regex regex;
#endif
        private readonly List<string> TEST_DEVICES = new List<string>()
        {
            "9ED87174BA40D80E",
            "03025839C6157A0A",
            "D3C91C4185B0B88C",
            "3D23082D9FB8C8ABA5FB7D57448E5365"
        };

        public AdMobHandler(AdvertisingModules moduleType) : base(moduleType) 
        {
#if UNITY_EDITOR
            regex = new Regex(ADMOB_GAMEOBJECT_NAME_REGEX);
#endif
        }

        public override void Init(AdsData adsSettings)
        {
            this.adsSettings = adsSettings;

            if (adsSettings.SystemLogs)
                Debug.Log("[AdsManager]: AdMob is trying to initialize!");

            MobileAds.SetiOSAppPauseOnBackground(true);

            RequestConfiguration.Builder requestBuilder = new RequestConfiguration.Builder();
            requestBuilder = requestBuilder.SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.Unspecified);
            requestBuilder = requestBuilder.SetTestDeviceIds(TEST_DEVICES);

            MobileAds.SetRequestConfiguration(requestBuilder.build());

            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize(InitCompleteAction);
        }

        private void InitCompleteAction(InitializationStatus initStatus)
        {
            GoogleMobileAds.Common.MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                // Get singleton reward based video ad reference.
                rewardBasedVideo = new RewardedAd(GetRewardedVideoID());

                // RewardBasedVideoAd is a singleton, so handlers should only be registered once.
                rewardBasedVideo.OnAdLoaded += HandleRewardBasedVideoLoaded;
                rewardBasedVideo.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
                rewardBasedVideo.OnAdFailedToShow += HandleRewardBasedVideoFailedToShow;
                rewardBasedVideo.OnAdOpening += HandleRewardBasedVideoOpened;
                rewardBasedVideo.OnUserEarnedReward += HandleRewardBasedVideoRewarded;
                rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;

                isInitialized = true;

                RequestRewardedVideo();
                RequestInterstitial();
                RequestBanner();

                if (AdsManager.OnAdsModuleInitializedEvent != null)
                    AdsManager.OnAdsModuleInitializedEvent.Invoke(ModuleType);

                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: AdMob is initialized!");
            });
        }

        public override void DestroyBanner()
        {
            if (bannerView != null)
                bannerView.Destroy();
        }

        public override void HideBanner()
        {
            if (bannerView != null)
                bannerView.Hide();
        }

        public override void RequestInterstitial()
        {
            if (!isInitialized)
                return;

            if(adsSettings.InterstitialType != AdvertisingModules.AdMob)
            {
                return;
            }

            if (!AdsManager.IsForcedAdEnabled(false))
            {
                return;
            }

            // Clean up interstitial ad before creating a new one.
            if (interstitial != null)
            {
                interstitial.Destroy();
            }

            // Create an interstitial.
            interstitial = new InterstitialAd(GetInterstitialID());

            // Register for ad events.
            interstitial.OnAdLoaded += HandleInterstitialLoaded;
            interstitial.OnAdFailedToLoad += HandleInterstitialFailedToLoad;
            interstitial.OnAdOpening += HandleInterstitialOpened;
            interstitial.OnAdClosed += HandleInterstitialClosed;

            // Load an interstitial ad.
            interstitial.LoadAd(GetAdRequest());
        }

        public override void RequestRewardedVideo()
        {
            if (!isInitialized)
                return;
            
            if(adsSettings.RewardedVideoType != AdvertisingModules.AdMob)
            {
                return;
            }

            rewardBasedVideo.LoadAd(new AdRequest.Builder().Build());
        }

        private void RequestBanner()
        {
            if(adsSettings.BannerType != AdvertisingModules.AdMob)
            {
                return;
            }

            if (!AdsManager.IsForcedAdEnabled(false))
            {
                return;
            }

            // Clean up banner before reusing
            if (bannerView != null)
            {
                bannerView.Destroy();
            }

            AdSize adSize = AdSize.Banner;

            switch (adsSettings.AdMobSettings.BannerType)
            {
                case AdMobData.BannerPlacementType.Banner:
                    adSize = AdSize.Banner;
                    break;
                case AdMobData.BannerPlacementType.MediumRectangle:
                    adSize = AdSize.MediumRectangle;
                    break;
                case AdMobData.BannerPlacementType.IABBanner:
                    adSize = AdSize.IABBanner;
                    break;
                case AdMobData.BannerPlacementType.Leaderboard:
                    adSize = AdSize.Leaderboard;
                    break;
                case AdMobData.BannerPlacementType.SmartBanner:
                    adSize = AdSize.SmartBanner;
                    break;
            }

            AdPosition adPosition = AdPosition.Bottom;
            switch (adsSettings.AdMobSettings.BannerPosition)
            {
                case BannerPosition.Bottom:
                    adPosition = AdPosition.Bottom;
                    break;
                case BannerPosition.Top:
                    adPosition = AdPosition.Top;
                    break;
            }

            bannerView = new BannerView(GetBannerID(), adSize, adPosition);

            // Register for ad events.
            bannerView.OnAdLoaded += HandleAdLoaded;
            bannerView.OnAdFailedToLoad += HandleAdFailedToLoad;
            bannerView.OnAdOpening += HandleAdOpened;
            bannerView.OnAdClosed += HandleAdClosed;

            // Load a banner ad.
            bannerView.LoadAd(GetAdRequest());
        }

        public override void ShowBanner()
        {
            if (!isInitialized)
                return;

            bannerView.Show();
        }

        public override void ShowInterstitial(InterstitialCallback callback)
        {
            if (!isInitialized)
            {
                if (callback != null)
                    callback.Invoke(false);

                return;
            }

            interstitial.Show();
        }

        public override void ShowRewardedVideo(RewardedVideoCallback callback)
        {
            if (!isInitialized)
                return;

            rewardBasedVideo.Show();
        }

        public override bool IsInterstitialLoaded()
        {
            return interstitial != null && interstitial.IsLoaded();
        }

        public override bool IsRewardedVideoLoaded()
        {
            return rewardBasedVideo != null && rewardBasedVideo.IsLoaded();
        }

        public override void SetGDPR(bool state)
        {

        }

        public override bool IsGDPRRequired()
        {
            return true;
        }

        public AdRequest GetAdRequest()
        {
            AdRequest.Builder builder = new AdRequest.Builder();

            builder = builder.AddExtra("npa", AdsManager.GetGDPRState() ? "1" : "0");

            return builder.Build();
        }

    #region Banner Callbacks
        public void HandleAdLoaded(object sender, System.EventArgs args)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: HandleAdLoaded event received");

                if (AdsManager.OnBannerAdLoadedEvent != null)
                    AdsManager.OnBannerAdLoadedEvent.Invoke();
            });
        }

        public void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: HandleFailedToReceiveAd event received with message: " + args.LoadAdError.GetMessage());

                if (AdsManager.OnBannerAdLoadFailedEvent != null)
                    AdsManager.OnBannerAdLoadFailedEvent.Invoke();
            });
        }

        public void HandleAdOpened(object sender, System.EventArgs args)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: HandleAdOpened event received");

                if (AdsManager.OnBannerAdClickedEvent != null)
                    AdsManager.OnBannerAdClickedEvent.Invoke();
            });
        }

        public void HandleAdClosed(object sender, System.EventArgs args)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: HandleAdClosed event received");
            });
        }

        public void HandleAdLeftApplication(object sender, System.EventArgs args)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: HandleAdLeftApplication event received");
            });
        }
    #endregion

    #region Interstitial Callback
        public void HandleInterstitialLoaded(object sender, System.EventArgs args)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: HandleInterstitialLoaded event received");

                interstitialRetryAttempt = RETRY_ATTEMPT_DEFAULT_VALUE;

                if (AdsManager.OnInterstitialLoadedEvent != null)
                    AdsManager.OnInterstitialLoadedEvent.Invoke();
            });
        }

        public void HandleInterstitialFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: HandleInterstitialFailedToLoad event received with message: " + args.LoadAdError.GetMessage());

                if (AdsManager.OnInterstitialLoadFailedEvent != null)
                    AdsManager.OnInterstitialLoadFailedEvent.Invoke();

                interstitialRetryAttempt++;
                float retryDelay = Mathf.Pow(2, interstitialRetryAttempt);

                Tween.DelayedCall(interstitialRetryAttempt, () => AdsManager.RequestInterstitial(), true, TweenType.Update);
            });
        }

        public void HandleInterstitialOpened(object sender, System.EventArgs args)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: HandleInterstitialOpened event received");

                if (AdsManager.OnInterstitialDisplayedEvent != null)
                    AdsManager.OnInterstitialDisplayedEvent.Invoke();
            });
        }

        public void HandleInterstitialClosed(object sender, System.EventArgs args)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: HandleInterstitialClosed event received");

                if (AdsManager.OnInterstitialHiddenEvent != null)
                    AdsManager.OnInterstitialHiddenEvent.Invoke();

                AdsManager.ExecuteInterstitialCallback(true);

                AdsManager.ResetInterstitialDelayTime();
                AdsManager.RequestInterstitial();
            });
        }

        public void HandleInterstitialLeftApplication(object sender, System.EventArgs args)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: HandleInterstitialLeftApplication event received");

                if (AdsManager.OnInterstitialClickedEvent != null)
                    AdsManager.OnInterstitialClickedEvent.Invoke();
            });
        }
    #endregion

    #region RewardedVideo Callback
        public void HandleRewardBasedVideoLoaded(object sender, System.EventArgs args)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: HandleRewardBasedVideoLoaded event received");

                rewardedRetryAttempt = RETRY_ATTEMPT_DEFAULT_VALUE;

                if (AdsManager.OnRewardedAdLoadedEvent != null)
                    AdsManager.OnRewardedAdLoadedEvent.Invoke();
            });
        }

        private void HandleRewardBasedVideoFailedToShow(object sender, AdErrorEventArgs args)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                AdsManager.ExecuteRewardVideoCallback(false);

                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: HandleRewardBasedVideoFailedToShow event received with message: " + args.AdError.GetMessage());

                rewardedRetryAttempt++;
                float retryDelay = Mathf.Pow(2, rewardedRetryAttempt);

                Tween.DelayedCall(rewardedRetryAttempt, () => AdsManager.RequestRewardBasedVideo(), true, TweenType.Update);

                if (AdsManager.OnRewardedAdLoadFailedEvent != null)
                    AdsManager.OnRewardedAdLoadFailedEvent.Invoke();
            });
        }

        public void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                AdsManager.ExecuteRewardVideoCallback(false);

                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: HandleRewardBasedVideoFailedToLoad event received with message: " + args.LoadAdError.GetMessage());

                rewardedRetryAttempt++;
                float retryDelay = Mathf.Pow(2, rewardedRetryAttempt);

                Tween.DelayedCall(rewardedRetryAttempt, () => AdsManager.RequestRewardBasedVideo(), true, TweenType.Update);

                if (AdsManager.OnRewardedAdLoadFailedEvent != null)
                    AdsManager.OnRewardedAdLoadFailedEvent.Invoke();
            });
        }

        public void HandleRewardBasedVideoOpened(object sender, System.EventArgs args)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: HandleRewardBasedVideoOpened event received");

#if UNITY_EDITOR
                //fix that helps display ads over store
                UnityEngine.Object[] canvases = GameObject.FindObjectsOfType(typeof(Canvas),false);

                for (int i = 0; i < canvases.Length; i++)
                {
                    if (regex.IsMatch(canvases[i].name))
                    {
                        ((Canvas)canvases[i]).sortingOrder = 9999;
                        break;
                    }
                }
#endif

                if (AdsManager.OnRewardedAdDisplayedEvent != null)
                    AdsManager.OnRewardedAdDisplayedEvent.Invoke();
            });

            isRewarded = false;
        }

        public void HandleRewardBasedVideoStarted(object sender, System.EventArgs args)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: HandleRewardBasedVideoStarted event received");
            });

            isRewarded = false;
        }

        public void HandleRewardBasedVideoClosed(object sender, System.EventArgs args)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: HandleRewardBasedVideoClosed event received");

                if (AdsManager.OnRewardedAdHiddenEvent != null)
                    AdsManager.OnRewardedAdHiddenEvent.Invoke();

                AdsManager.ResetInterstitialDelayTime();
                AdsManager.RequestRewardBasedVideo();
            });

            Tween.NextFrame( () =>
            {
                AdsManager.CallEventInMainThread(delegate
                {
                    AdsManager.ExecuteRewardVideoCallback(isRewarded);
                    isRewarded = false;
                });
            });
        }

        public void HandleRewardBasedVideoRewarded(object sender, GoogleMobileAds.Api.Reward args)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                //AdsManager.ExecuteRewardVideoCallback(true);
                isRewarded = true;

                string type = args.Type;
                double amount = args.Amount;

                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: HandleRewardBasedVideoRewarded event received for " + amount.ToString() + " " + type);

                if (AdsManager.OnRewardedAdReceivedRewardEvent != null)
                    AdsManager.OnRewardedAdReceivedRewardEvent.Invoke();
            });
        }

        public void HandleRewardBasedVideoLeftApplication(object sender, System.EventArgs args)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: HandleRewardBasedVideoLeftApplication event received");

                if (AdsManager.OnRewardedAdClickedEvent != null)
                    AdsManager.OnRewardedAdClickedEvent.Invoke();
            });
        }
    #endregion

        public override string GetBannerID()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.AdMobSettings.AndroidBannerID;
#elif UNITY_IOS
            return adsSettings.AdMobSettings.IOSBannerID;
#else
            return "unexpected_platform";
#endif
        }

        public override string GetInterstitialID()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.AdMobSettings.AndroidInterstitialID;
#elif UNITY_IOS
            return adsSettings.AdMobSettings.IOSInterstitialID;
#else
            return "unexpected_platform";
#endif
        }

        public override string GetRewardedVideoID()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.AdMobSettings.AndroidRewardedVideoID;
#elif UNITY_IOS
            return adsSettings.AdMobSettings.IOSRewardedVideoID;
#else
            return "unexpected_platform";
#endif
        }
    }
#endif
}

// -----------------
// Advertisement v 1.2.1
// -----------------