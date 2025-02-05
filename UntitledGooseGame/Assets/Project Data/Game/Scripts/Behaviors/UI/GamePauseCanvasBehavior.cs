#pragma warning disable 0649

using UnityEngine;
using UnityEngine.UI;
using StarkSDKSpace;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;

namespace Watermelon
{
    public class GamePauseCanvasBehavior : MonoBehaviour
    {

        private static GamePauseCanvasBehavior instance;

        [Header("Canvas")]
        [SerializeField] Canvas canvas;
        [SerializeField] CanvasGroup backCanvasGroup;
        [SerializeField] GraphicRaycaster raycaster;

        [Header("Panel")]
        [SerializeField] Image pauseText;
        [SerializeField] Image adsButton;
        [SerializeField] Image homeButton;
        [SerializeField] Image playButton;
        [SerializeField] Image replayButton;

        [Header("Ad Button")]
        [SerializeField] Text coinsForAdsText;
        [SerializeField] Image adsimage;
        [SerializeField] Image coinImage;

        [Header("Coins")]
        [SerializeField] RectTransform coinsPanel;
        [SerializeField] Text coinsAmount;

        private static Canvas Canvas => instance.canvas;
        private static CanvasGroup BackCanvasGroup => instance.backCanvasGroup;
        private static GraphicRaycaster Raycaster => instance.raycaster;

        private static Image PauseText => instance.pauseText;
        private static Image AdsButton => instance.adsButton;
        private static Image HomeButtonImage => instance.homeButton;
        private static Image PlayButton => instance.playButton;
        private static Image ReplayButtonImage => instance.replayButton;

        private static Text CoinsForAdsText => instance.coinsForAdsText;
        private static Image Adsimage => instance.adsimage;
        private static Image CoinImage => instance.coinImage;

        private static RectTransform CoinsPanel => instance.coinsPanel;
        private static Text CoinsAmount => instance.coinsAmount;

        private StarkAdManager starkAdManager;

        public string clickid;
        private void Awake()
        {
            instance = this;
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
        public void HomeButton()
        {
            GoHome();
        }

        public void GoHome()
        {
            ShowInterstitialAd("1lcaf5895d5l1293dc",
            () => {
                Debug.LogError("--插屏广告完成--");

            },
            (it, str) => {
                Debug.LogError("Error->" + str);
            });
            CloseButton();
            GameCanvasBehavior.Hide();

            LevelController.DisposeLevel();

            Tween.DelayedCall(0.5f, () => { MainMenuBehavior.Show(true); });
        }

        public void ReplayButton()
        {
            AdsManager.ShowInterstitial(ReplayLevel); 
        }

        public void ReplayLevel(bool interstitialWasDisplayer)
        {
            CloseButton();
            LevelController.DisposeLevel();

            Tween.DelayedCall(1f, () => LevelController.LoadLevel(LevelController.Level));
        }

        public void CoinsForAdButton()
        {
            AdsManager.ShowRewardBasedVideo((finished) =>
            {
                if (finished)
                {
                    GameController.Coins += GameSettings.CoinsForAd;

                    CoinsAmount.text = GameController.Coins.ToString();
                }
            });
        }

        public void CloseButton()
        {
            PauseText.transform.DOScale(0, 0.5f).SetEasing(Ease.Type.SineInOut);
            AdsButton.transform.DOScale(0, 0.5f).SetEasing(Ease.Type.SineInOut);
            HomeButtonImage.transform.DOScale(0, 0.5f).SetEasing(Ease.Type.SineInOut);
            PlayButton.transform.DOScale(0, 0.5f).SetEasing(Ease.Type.SineInOut);
            ReplayButtonImage.transform.DOScale(0, 0.5f).SetEasing(Ease.Type.SineInOut);
            CoinsPanel.transform.DOScale(0, 0.5f).SetEasing(Ease.Type.SineInOut);

            AdsManager.HideBanner();
            Raycaster.enabled = false;

            BackCanvasGroup.DOFade(0, 0.5f).OnComplete(() =>
            {
                BackCanvasGroup.gameObject.SetActive(false);
                Canvas.enabled = false;

                GameCanvasBehavior.RaycasterEnabled = true;
            });
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

            PauseText.transform.localScale = Vector3.zero;
            AdsButton.transform.localScale = Vector3.zero;
            HomeButtonImage.transform.localScale = Vector3.zero;
            PlayButton.transform.localScale = Vector3.zero;
            ReplayButtonImage.transform.localScale = Vector3.zero;
            CoinsPanel.transform.localScale = Vector3.zero;

            BackCanvasGroup.gameObject.SetActive(true);
            BackCanvasGroup.alpha = 0;
            BackCanvasGroup.DOFade(1, 0.5f).OnComplete(() => {

                PauseText.transform.DOScale(1, 0.5f).SetEasing(Ease.Type.SineInOut);
                AdsButton.transform.DOScale(1, 0.5f).SetEasing(Ease.Type.SineInOut);
                HomeButtonImage.transform.DOScale(1, 0.5f).SetEasing(Ease.Type.SineInOut);
                PlayButton.transform.DOScale(1, 0.5f).SetEasing(Ease.Type.SineInOut);
                ReplayButtonImage.transform.DOScale(1, 0.5f).SetEasing(Ease.Type.SineInOut);
                CoinsPanel.transform.DOScale(1, 0.5f).SetEasing(Ease.Type.SineInOut);

                AdsManager.ShowBanner();

                Raycaster.enabled = true;
            });

            CoinsAmount.text = GameController.Coins.ToString();
        }
    }
}