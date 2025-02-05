#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class MainMenuBehavior : MonoBehaviour
    {
        private static readonly int TILIND_ID = Shader.PropertyToID("_Tiling");

        private static MainMenuBehavior instance;

        [Header("Canvases")]
        [SerializeField] Canvas mainMenuCanvas;
        [SerializeField] CanvasGroup mainMenuCanvasGroup;
        [SerializeField] GraphicRaycaster mainMenuRaycaster;

        [Space]
        [SerializeField] CanvasScaler viewportCanvasScaler;

        [Header("Main Panel")]
        [SerializeField] Image title;
        [SerializeField] SettingsPanel settings;
        [SerializeField] Text coinsAmount;
        [SerializeField] Text coinsForAdText;

        [Header("Backgrounds")]
        [SerializeField] Image topBackground;
        [SerializeField] Image middleBackground;
        [SerializeField] Image bottomBackground;
        [SerializeField] Image movingBackground;

        [Space]
        [SerializeField] Material movingBackgroundMaterial;

        public static int CoinsAmount { set => instance.coinsAmount.text = value.ToString(); }

        private static Canvas MainMenuCanvas => instance.mainMenuCanvas;
        private static CanvasGroup MainMenuCanvasGroup => instance.mainMenuCanvasGroup;

        private static CanvasScaler ViewportCanvasScaler => instance.viewportCanvasScaler;

        private static Image TopBackground => instance.topBackground;
        private static Image MiddleBackground => instance.middleBackground;
        private static Text CoinsForAdsText => instance.coinsForAdText;
        private static GraphicRaycaster MainMenuRaycaster => instance.mainMenuRaycaster;

        private static SettingsPanel Settings => instance.settings;

        private static Material MovingBackgroundMaterial => instance.movingBackgroundMaterial;

        private void Awake()
        {
            instance = this;

            float screenRatio = Screen.width / (float)Screen.height;

            SetupUIForScreenRatio(viewportCanvasScaler);

            MovingBackgroundMaterial.SetVector(TILIND_ID, new Vector2(screenRatio * 2, 2));

            CoinsForAdsText.text = "+" + GameSettings.CoinsForAd;
        }

        public static bool IsTablet()
        {
#if UNITY_IOS
            bool deviceIsIpad = UnityEngine.iOS.Device.generation.ToString().Contains("iPad");
            if (deviceIsIpad)
                return true;
 
            bool deviceIsIphone = UnityEngine.iOS.Device.generation.ToString().Contains("iPhone");
            if (deviceIsIphone)
                return false;
            return false;
#else
            float screenRatio = Screen.width / (float)Screen.height;

            return screenRatio > ViewportCanvasScaler.referenceResolution.x / ViewportCanvasScaler.referenceResolution.y + 0.1f;
#endif
        }

        private void SetupUIForScreenRatio(CanvasScaler canvasScaler)
        {
            if (IsTablet())
            {
                canvasScaler.matchWidthOrHeight = 1f;
            }
        }

        public static void Show(bool withWaves = false)
        {
            MainMenuCanvas.enabled = true;
            MainMenuCanvasGroup.alpha = 0;
            MainMenuCanvasGroup.DOFade(1, 0.4f).OnComplete(() => {
                AdsManager.ShowBanner();

                MainMenuRaycaster.enabled = true;
            });

            if (withWaves)
            {
                MiddleBackground.rectTransform.DOAnchoredPosition(new Vector2(0, 0), 0.5f).SetEasing(Ease.Type.SineOut);

                Tween.DelayedCall(0.2f, () => TopBackground.rectTransform.DOAnchoredPosition(new Vector2(0, 0), 0.5f).SetEasing(Ease.Type.SineOut));
            }
        }

        public static void Hide(bool hideBanner)
        {
            if(hideBanner) AdsManager.HideBanner();

            if (Settings.IsActiveSettingsButton) SettingsPanel.HidePanel(true);

            MainMenuRaycaster.enabled = false;

            //Title.DOFade(0, 0.5f);

            TopBackground.rectTransform.DOAnchoredPosition(new Vector2(0, -850), 0.5f).SetEasing(Ease.Type.SineIn);

            Tween.DelayedCall(0.2f, () => MiddleBackground.rectTransform.DOAnchoredPosition(new Vector2(0, -1180), 0.5f).SetEasing(Ease.Type.SineIn));
            
            MainMenuCanvasGroup.alpha = 1;
            MainMenuCanvasGroup.DOFade(0, 0.5f).OnComplete(() => {
                MainMenuCanvas.enabled = false;
            });
        }

        public void PlayButton()
        {
            Hide(true);
            Tween.DelayedCall(0.7f, GameController.Play);
        }

        public void SelectLevelButton()
        {
            Hide(false);
            Tween.DelayedCall(0.5f, LevelSelectionBehavior.Show);
        }

        public void WatchAdForCoins()
        {
            AdsManager.ShowRewardBasedVideo((finished) =>
            {
                if (finished)
                {
                    GameController.Coins += GameSettings.CoinsForAd;
                }
            });
        }

        public void DailyRewardButton()
        {
            Hide(false);
            Tween.DelayedCall(0.5f,() => {
                DailyRewardController.Show();
            });
        }

        public void Update()
        {
            //if (GameController.ShouldOpenDaily && MainMenuRaycaster.enabled)
            //{
            //    if (AdsManager.IsRewardBasedVideoLoaded())
            //    {
            //        GameController.ShouldOpenDaily = false;

            //        Hide(false);
            //        DailyRewardController.Show();
            //    }
            //}
        }
    }
}