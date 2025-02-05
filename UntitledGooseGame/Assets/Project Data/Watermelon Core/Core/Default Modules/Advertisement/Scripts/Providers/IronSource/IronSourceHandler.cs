using UnityEngine;

namespace Watermelon
{
#if MODULE_IRONSOURCE
    public class IronSourceHandler : AdvertisingHandler
    {
        private const int RETRY_ATTEMPT_DEFAULT_VALUE = 1;

        private int interstitialRetryAttempt = RETRY_ATTEMPT_DEFAULT_VALUE;
        private int rewardedRetryAttempt = RETRY_ATTEMPT_DEFAULT_VALUE;

        private bool isBannerLoaded = false;

        private IronSourceListner eventsHolder;

        public static OnApplicationPaused OnApplicationPausedCallback;
        public delegate void OnApplicationPaused(bool isPaused);

        public IronSourceHandler(AdvertisingModules moduleType) : base(moduleType) { }

        public override void Init(AdsData adsSettings)
        {
            this.adsSettings = adsSettings;

            SubscribeEvents();

            eventsHolder = Initialiser.InitialiserGameObject.AddComponent<IronSourceListner>();
            eventsHolder.Init(this);

            //IronSource.Agent.setAdaptersDebug(adsSettings.TestMode);

            if (adsSettings.SystemLogs)
                Debug.Log("[AdsManager]: IronSource.Agent.validateIntegration");

            IronSource.Agent.validateIntegration();

            if (adsSettings.SystemLogs)
                Debug.Log("[AdsManager]: Unity version" + IronSource.unityVersion());

            if (adsSettings.SystemLogs)
                Debug.Log("[AdsManager]: IronSource.Agent.init");

            string appKey = GetAppKey();

            IronSource.Agent.init(appKey);

            //if(adsSettings.TestMode)
            //    IronSource.Agent.validateIntegration();

            isInitialized = true;
        }

        private void SubscribeEvents()
        {
            if (adsSettings.RewardedVideoType == AdvertisingModules.IronSource)
            {
                //Add Rewarded Video Events
                IronSourceEvents.onRewardedVideoAdOpenedEvent += RewardedVideoAdOpenedEvent;
                IronSourceEvents.onRewardedVideoAdClosedEvent += RewardedVideoAdClosedEvent;
                IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardedVideoAvailabilityChangedEvent;
                IronSourceEvents.onRewardedVideoAdStartedEvent += RewardedVideoAdStartedEvent;
                IronSourceEvents.onRewardedVideoAdEndedEvent += RewardedVideoAdEndedEvent;
                IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent;
                IronSourceEvents.onRewardedVideoAdShowFailedEvent += RewardedVideoAdShowFailedEvent;
                IronSourceEvents.onRewardedVideoAdClickedEvent += RewardedVideoAdClickedEvent;
            }

            if (adsSettings.InterstitialType == AdvertisingModules.IronSource)
            {
                // Add Interstitial Events
                IronSourceEvents.onInterstitialAdReadyEvent += InterstitialAdReadyEvent;
                IronSourceEvents.onInterstitialAdLoadFailedEvent += InterstitialAdLoadFailedEvent;
                IronSourceEvents.onInterstitialAdShowSucceededEvent += InterstitialAdShowSucceededEvent;
                IronSourceEvents.onInterstitialAdShowFailedEvent += InterstitialAdShowFailedEvent;
                IronSourceEvents.onInterstitialAdClickedEvent += InterstitialAdClickedEvent;
                IronSourceEvents.onInterstitialAdOpenedEvent += InterstitialAdOpenedEvent;
                IronSourceEvents.onInterstitialAdClosedEvent += InterstitialAdClosedEvent;
            }

            if (adsSettings.BannerType == AdvertisingModules.IronSource)
            {
                // Add Banner Events
                IronSourceEvents.onBannerAdLoadedEvent += BannerAdLoadedEvent;
                IronSourceEvents.onBannerAdLoadFailedEvent += BannerAdLoadFailedEvent;
                IronSourceEvents.onBannerAdClickedEvent += BannerAdClickedEvent;
                IronSourceEvents.onBannerAdScreenPresentedEvent += BannerAdScreenPresentedEvent;
                IronSourceEvents.onBannerAdScreenDismissedEvent += BannerAdScreenDismissedEvent;
                IronSourceEvents.onBannerAdLeftApplicationEvent += BannerAdLeftApplicationEvent;
            }

            //Add ImpressionSuccess Event
            IronSourceEvents.onImpressionSuccessEvent += ImpressionSuccessEvent;
        }

    #region RewardedAd callback handlers
        private void RewardedVideoAvailabilityChangedEvent(bool canShowAd)
        {
            if (adsSettings.SystemLogs)
                Debug.Log("[AdsManager]: RewardedVideoAvailabilityChangedEvent event received - " + canShowAd);

            if(canShowAd)
            {
                rewardedRetryAttempt = RETRY_ATTEMPT_DEFAULT_VALUE;

                if (AdsManager.OnRewardedAdLoadedEvent != null)
                    AdsManager.OnRewardedAdLoadedEvent.Invoke();
            }
        }

        private void RewardedVideoAdOpenedEvent()
        {
            if (adsSettings.SystemLogs)
                Debug.Log("[AdsManager]: RewardedVideoAdOpenedEvent event received");

            if (AdsManager.OnRewardedAdDisplayedEvent != null)
                AdsManager.OnRewardedAdDisplayedEvent.Invoke();
        }

        private void RewardedVideoAdRewardedEvent(IronSourcePlacement ssp)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                AdsManager.ExecuteRewardVideoCallback(true);

                string type = ssp.getRewardName();
                int amount = ssp.getRewardAmount();

                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: HandleRewardBasedVideoRewarded event received for " + amount.ToString() + " " + type);

                if (AdsManager.OnRewardedAdReceivedRewardEvent != null)
                    AdsManager.OnRewardedAdReceivedRewardEvent.Invoke();
            });
        }

        private void RewardedVideoAdClosedEvent()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                AdsManager.ExecuteRewardVideoCallback(false);

                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: RewardedVideoAdClosedEvent event received");

                if (AdsManager.OnRewardedAdHiddenEvent != null)
                    AdsManager.OnRewardedAdHiddenEvent.Invoke();

                AdsManager.ResetInterstitialDelayTime();
                AdsManager.RequestRewardBasedVideo();
            });
        }

        private void RewardedVideoAdStartedEvent()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: RewardedVideoAdStartedEvent event received");
            });
        }

        private void RewardedVideoAdEndedEvent()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: RewardedVideoAdEndedEvent event received");
            });
        }

        private void RewardedVideoAdShowFailedEvent(IronSourceError error)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: RewardedVideoAdShowFailedEvent event received " + error.getDescription());
            });
        }

        private void RewardedVideoAdClickedEvent(IronSourcePlacement ssp)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: RewardedVideoAdClickedEvent event received");
            });
        }
    #endregion

    #region Interstitial callback handlers
        private void InterstitialAdReadyEvent()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: InterstitialAdReadyEvent event received");

                interstitialRetryAttempt = RETRY_ATTEMPT_DEFAULT_VALUE;

                if (AdsManager.OnInterstitialLoadedEvent != null)
                    AdsManager.OnInterstitialLoadedEvent.Invoke();
            });
        }

        private void InterstitialAdLoadFailedEvent(IronSourceError error)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: InterstitialAdLoadFailedEvent event received with message: " + error.getDescription());

                if (AdsManager.OnInterstitialLoadFailedEvent != null)
                    AdsManager.OnInterstitialLoadFailedEvent.Invoke();

                interstitialRetryAttempt++;
                float retryDelay = Mathf.Pow(2, interstitialRetryAttempt);

                Tween.DelayedCall(interstitialRetryAttempt, () => AdsManager.RequestInterstitial(), true, TweenType.Update);
            });
        }

        private void InterstitialAdShowSucceededEvent()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: InterstitialAdShowSucceededEvent event received");

                if (AdsManager.OnInterstitialDisplayedEvent != null)
                    AdsManager.OnInterstitialDisplayedEvent.Invoke();
            });
        }

        private void InterstitialAdShowFailedEvent(IronSourceError error)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: InterstitialAdShowFailedEvent event received with message: " + error.getDescription());

                AdsManager.ExecuteInterstitialCallback(false);

                AdsManager.ResetInterstitialDelayTime();
                AdsManager.RequestInterstitial();
            });
        }

        private void InterstitialAdClickedEvent()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: InterstitialAdClickedEvent event received");
            });
        }

        private void InterstitialAdOpenedEvent()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: InterstitialAdOpenedEvent event received");
            });
        }

        private void InterstitialAdClosedEvent()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: InterstitialAdClosedEvent event received");

                if (AdsManager.OnInterstitialHiddenEvent != null)
                    AdsManager.OnInterstitialHiddenEvent.Invoke();

                AdsManager.ExecuteInterstitialCallback(true);

                AdsManager.ResetInterstitialDelayTime();
                AdsManager.RequestInterstitial();
            });
        }
    #endregion

    #region Banner callback handlers
        private void BannerAdLoadedEvent()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: BannerAdLoadedEvent event received");

                if (AdsManager.OnBannerAdLoadedEvent != null)
                    AdsManager.OnBannerAdLoadedEvent.Invoke();
            });
        }

        private void BannerAdLoadFailedEvent(IronSourceError error)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: BannerAdLoadFailedEvent event received with message: " + error.getDescription());

                if (AdsManager.OnBannerAdLoadFailedEvent != null)
                    AdsManager.OnBannerAdLoadFailedEvent.Invoke();
            });
        }

        private void BannerAdClickedEvent()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: BannerAdClickedEvent event received");

                if (AdsManager.OnBannerAdClickedEvent != null)
                    AdsManager.OnBannerAdClickedEvent.Invoke();
            });
        }

        private void BannerAdScreenPresentedEvent()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: BannerAdScreenPresentedEvent event received");
            });
        }

        private void BannerAdScreenDismissedEvent()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: BannerAdScreenDismissedEvent event received");
            });
        }

        private void BannerAdLeftApplicationEvent()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: BannerAdLeftApplicationEvent event received");
            });
        }
    #endregion

    #region ImpressionSuccess callback handler
        private void ImpressionSuccessEvent(IronSourceImpressionData impressionData)
        {
            if (adsSettings.SystemLogs)
            {
                Debug.Log("unity - script: I got ImpressionSuccessEvent ToString(): " + impressionData.ToString());
                Debug.Log("unity - script: I got ImpressionSuccessEvent allData: " + impressionData.allData);
            }
        }
    #endregion

        public override void DestroyBanner()
        {
            if (!isInitialized)
                return;

            IronSource.Agent.destroyBanner();

            isBannerLoaded = false;
        }

        public override void HideBanner()
        {
            if (!isInitialized)
                return;

            if(isBannerLoaded)
                IronSource.Agent.hideBanner();
        }

        public override void RequestInterstitial()
        {
            if (!isInitialized)
                return;

            if(!IronSource.Agent.isInterstitialReady())
                IronSource.Agent.loadInterstitial();
        }

        public override void RequestRewardedVideo()
        {
            // Do nothing
        }

        public override void ShowBanner()
        {
            if (!isInitialized)
                return;

            if(!isBannerLoaded)
            {
                IronSourceBannerSize ironSourceBannerSize = IronSourceBannerSize.BANNER;
                switch (adsSettings.IronSourceSettings.BannerType)
                {
                    case IronSourceContainer.BannerPlacementType.Large:
                        ironSourceBannerSize = IronSourceBannerSize.LARGE;
                        break;
                    case IronSourceContainer.BannerPlacementType.Rectangle:
                        ironSourceBannerSize = IronSourceBannerSize.RECTANGLE;
                        break;
                    case IronSourceContainer.BannerPlacementType.Smart:
                        ironSourceBannerSize = IronSourceBannerSize.SMART;
                        break;
                }

                IronSourceBannerPosition ironSourceBannerPosition = IronSourceBannerPosition.BOTTOM;
                if (adsSettings.IronSourceSettings.BannerPosition == BannerPosition.Top)
                    ironSourceBannerPosition = IronSourceBannerPosition.TOP;

                IronSource.Agent.loadBanner(ironSourceBannerSize, ironSourceBannerPosition);

                isBannerLoaded = true;
            }
            else
            {
                IronSource.Agent.displayBanner();
            }
        }

        public override void ShowInterstitial(InterstitialCallback callback)
        {
            if (!isInitialized)
            {
                if(callback != null)
                    callback.Invoke(false);

                return;
            }

            IronSource.Agent.showInterstitial();
        }

        public override void ShowRewardedVideo(RewardedVideoCallback callback)
        {
            if (!isInitialized)
                return;

            IronSource.Agent.showRewardedVideo();
        }

        public override bool IsInterstitialLoaded()
        {
            return IronSource.Agent.isInterstitialReady();
        }

        public override bool IsRewardedVideoLoaded()
        {
            return IronSource.Agent.isRewardedVideoAvailable();
        }

        public override void SetGDPR(bool state)
        {
            IronSource.Agent.setConsent(state);
        }

        public override bool IsGDPRRequired()
        {
            return true;
        }

        public string GetAppKey()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.IronSourceSettings.AndroidAppKey;
#elif UNITY_IOS
            return adsSettings.IronSourceSettings.IOSAppKey;
#else
            return "unexpected_platform";
#endif
        }

        public override string GetBannerID()
        {
            return "";
        }

        public override string GetInterstitialID()
        {
            return "";
        }

        public override string GetRewardedVideoID()
        {
            return "";
        }

        private class IronSourceListner : MonoBehaviour
        {
            private IronSourceHandler ironSourceHandler;

            public void Init(IronSourceHandler ironSourceHandler)
            {
                this.ironSourceHandler = ironSourceHandler;
            }

            private void OnApplicationPause(bool isPaused)
            {
                if (ironSourceHandler.adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: OnApplicationPause = " + isPaused);

                IronSource.Agent.onApplicationPause(isPaused);

                OnApplicationPausedCallback?.Invoke(isPaused);
            }
        }
    }
#endif
}

// -----------------
// Advertisement v 1.2.1
// -----------------