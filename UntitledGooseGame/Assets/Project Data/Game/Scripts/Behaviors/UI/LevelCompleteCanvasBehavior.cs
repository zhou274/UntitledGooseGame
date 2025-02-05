#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StarkSDKSpace;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;



namespace Watermelon
{
    public class LevelCompleteCanvasBehavior : MonoBehaviour
    {
        private static LevelCompleteCanvasBehavior instance;

        [Header("Canvas")]
        [SerializeField] Canvas canvas;
        [SerializeField] CanvasGroup backCanvasGroup;
        [SerializeField] GraphicRaycaster raycaster;

        [Header("Panel")]
        [SerializeField] Image levelCompleteText;
        [SerializeField] Image adsButton;
        [SerializeField] Image homeButton;
        [SerializeField] Image playButton;
        [SerializeField] Image replayButton;

        [Header("Ad Button")]
        [SerializeField] Text coinsForAdsText;
        [SerializeField] Image adsimage;
        [SerializeField] Image coinImage;

        private static Canvas Canvas => instance.canvas;
        private static CanvasGroup BackCanvasGroup => instance.backCanvasGroup;
        private static GraphicRaycaster Raycaster => instance.raycaster;

        private static Image LevelCompleteText => instance.levelCompleteText;
        private static Image AdsButton => instance.adsButton;
        private static Image HomeButtonImage => instance.homeButton;
        private static Image PlayButton => instance.playButton;
        private static Image ReplayButtonImage => instance.replayButton;

        private static Text CoinsForAdsText => instance.coinsForAdsText;
        private static Image Adsimage => instance.adsimage;
        private static Image CoinImage => instance.coinImage;

        private StarkAdManager starkAdManager;

        public string clickid;
        private void Awake()
        {
            instance = this;
        }

        public void HomeButton()
        {
            GoHome();
            ShowInterstitialAd("1lcaf5895d5l1293dc",
            () => {
                Debug.LogError("--插屏广告完成--");

            },
            (it, str) => {
                Debug.LogError("Error->" + str);
            });
        }

        public void GoHome()
        {
            Hide();
            GameCanvasBehavior.Hide();

            GameController.LevelComplete();

            Tween.DelayedCall(0.5f, () => { MainMenuBehavior.Show(true); });
        }

        public void ReplayButton()
        {
            AdsManager.ShowInterstitial(ReplayLevel);
        }

        private void ReplayLevel(bool interstitialWasDisplayer)
        {
            Hide();

            GameController.LevelComplete();

            Tween.DelayedCall(1f, () => LevelController.LoadLevel(LevelController.Level));
        }

        public void CoinsForAdButton()
        {
            AdsManager.ShowRewardBasedVideo((finished) =>
            {
                if (finished)
                {
                    GameController.Coins += GameSettings.CoinsForAd;
                }
            });
        }

        public void PlayNextLevelButton()
        {
            AdsManager.ShowInterstitial(NextLevel);
        }

        private void NextLevel(bool interstitialWasDisplayer)
        {
            Hide();

            Tween.DelayedCall(0.5f, GameController.FinishLevel);
        }

        public static void Show()
        {
            Canvas.enabled = true;

            CoinsForAdsText.text = "+" + GameSettings.CoinsForAd;

            Tween.NextFrame(() => {

                float halfSize = CoinsForAdsText.preferredWidth / 2f;
                CoinImage.rectTransform.anchoredPosition = new Vector2(halfSize + 20, 0);
                Adsimage.rectTransform.anchoredPosition = new Vector2(-halfSize, 0);
            });

            LevelCompleteText.transform.localScale = Vector3.zero;
            AdsButton.transform.localScale = Vector3.zero;
            HomeButtonImage.transform.localScale = Vector3.zero;
            PlayButton.transform.localScale = Vector3.zero;
            ReplayButtonImage.transform.localScale = Vector3.zero;

            BackCanvasGroup.gameObject.SetActive(true);
            BackCanvasGroup.alpha = 0;
            BackCanvasGroup.DOFade(1, 0.5f).OnComplete(() => {
                
                LevelCompleteText.transform.DOScale(1, 0.5f).SetEasing(Ease.Type.SineInOut);
                AdsButton.transform.DOScale(1, 0.5f).SetEasing(Ease.Type.SineInOut);
                HomeButtonImage.transform.DOScale(1, 0.5f).SetEasing(Ease.Type.SineInOut);
                PlayButton.transform.DOScale(1, 0.5f).SetEasing(Ease.Type.SineInOut);
                ReplayButtonImage.transform.DOScale(1, 0.5f).SetEasing(Ease.Type.SineInOut);

                AdsManager.ShowBanner();

                Raycaster.enabled = true;
            });
        }

        public static void Hide()
        {
            LevelCompleteText.transform.DOScale(0, 0.5f).SetEasing(Ease.Type.SineInOut);
            AdsButton.transform.DOScale(0, 0.5f).SetEasing(Ease.Type.SineInOut);
            HomeButtonImage.transform.DOScale(0, 0.5f).SetEasing(Ease.Type.SineInOut);
            PlayButton.transform.DOScale(0, 0.5f).SetEasing(Ease.Type.SineInOut);
            ReplayButtonImage.transform.DOScale(0, 0.5f).SetEasing(Ease.Type.SineInOut);

            AdsManager.HideBanner();
            Raycaster.enabled = false;

            BackCanvasGroup.DOFade(0, 0.5f).OnComplete(() =>
            {
                BackCanvasGroup.gameObject.SetActive(false);
                Canvas.enabled = false;
            });
        }
        /// <summary>
        /// 播放插屏广告
        /// </summary>
        /// <param name="adId"></param>
        /// <param name="errorCallBack"></param>
        /// <param name="closeCallBack"></param>
        public void ShowInterstitialAd(string adId, System.Action closeCallBack, System.Action<int, string> errorCallBack)
        {
            starkAdManager = StarkSDK.API.GetStarkAdManager();
            if (starkAdManager != null)
            {
                var mInterstitialAd = starkAdManager.CreateInterstitialAd(adId, errorCallBack, closeCallBack);
                mInterstitialAd.Load();
                mInterstitialAd.Show();
            }
        }
    }
}
