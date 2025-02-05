#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;
using StarkSDKSpace;


namespace Watermelon
{
    public class LevelFailedCanvasBehavior : MonoBehaviour
    {
        private static LevelFailedCanvasBehavior instance;

        [Header("Canvas")]
        [SerializeField] Canvas canvas;
        [SerializeField] CanvasGroup backCanvasGroup;
        [SerializeField] GraphicRaycaster raycaster;

        [Header("Panel")]
        [SerializeField] Image levelFailedText;
        [SerializeField] Image extraChanceButton;
        [SerializeField] Image homeButton;
        [SerializeField] Image replayButton;

        private static Canvas Canvas => instance.canvas;
        private static CanvasGroup BackCanvasGroup => instance.backCanvasGroup;
        private static GraphicRaycaster Raycaster => instance.raycaster;

        private static Image LevelFailedText => instance.levelFailedText;
        private static Image ExtraChanceImage => instance.extraChanceButton;
        private static Image HomeButtonImage => instance.homeButton;
        private static Image ReplayButtonImage => instance.replayButton;

        public string clickid;
        private StarkAdManager starkAdManager;
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

        private void GoHome()
        {
            Hide();
            GameCanvasBehavior.Hide();

            LevelController.DisposeLevel();

            Tween.DelayedCall(0.5f, () => { MainMenuBehavior.Show(true); });
        }

        public void ExtraChanceButton()
        {
            ShowVideoAd("192if3b93qo6991ed0",
            (bol) => {
                if (bol)
                {
                    Hide();
                    Tween.DelayedCall(0.5f, SlotsController.ReturnThreeLast);



                    clickid = "";
                    getClickid();
                    apiSend("game_addiction", clickid);
                    apiSend("lt_roi", clickid);


                }
                else
                {
                    StarkSDKSpace.AndroidUIManager.ShowToast("观看完整视频才能获取奖励哦！");
                }
            },
            (it, str) => {
                Debug.LogError("Error->" + str);
                //AndroidUIManager.ShowToast("广告加载异常，请重新看广告！");
            });
            
            //AdsManager.ShowInterstitial(ExtraChance);
        }

        private void ExtraChance(bool interstitialWasShown)
        {
            Hide();
            Tween.DelayedCall(0.5f, SlotsController.ReturnThreeLast);
        }

        public void TryAgainButton()
        {
            AdsManager.ShowInterstitial(ReplayLevel);
        }

        private void ReplayLevel(bool interstitialWasShown)
        {
            Hide();
            LevelController.DisposeLevel();

            Tween.DelayedCall(1f, () => LevelController.LoadLevel(LevelController.Level));
        }

        public static void Show()
        {

            Canvas.enabled = true;

            LevelFailedText.transform.localScale = Vector3.zero;
            ExtraChanceImage.transform.localScale = Vector3.zero;
            HomeButtonImage.transform.localScale = Vector3.zero;
            ReplayButtonImage.transform.localScale = Vector3.zero;

            BackCanvasGroup.gameObject.SetActive(true);
            BackCanvasGroup.alpha = 0;
            BackCanvasGroup.DOFade(1, 0.5f).OnComplete(() => {

                LevelFailedText.transform.DOScale(1, 0.5f).SetEasing(Ease.Type.SineInOut);
                ExtraChanceImage.transform.DOScale(1, 0.5f).SetEasing(Ease.Type.SineInOut);
                HomeButtonImage.transform.DOScale(1, 0.5f).SetEasing(Ease.Type.SineInOut);
                ReplayButtonImage.transform.DOScale(1, 0.5f).SetEasing(Ease.Type.SineInOut);
                
                AdsManager.ShowBanner();

                Raycaster.enabled = true;

            });
        }

        public static void Hide()
        {
            LevelFailedText.transform.DOScale(0, 0.5f).SetEasing(Ease.Type.SineInOut);
            ExtraChanceImage.transform.DOScale(0, 0.5f).SetEasing(Ease.Type.SineInOut);
            HomeButtonImage.transform.DOScale(0, 0.5f).SetEasing(Ease.Type.SineInOut);
            ReplayButtonImage.transform.DOScale(0, 0.5f).SetEasing(Ease.Type.SineInOut);

            AdsManager.HideBanner();

            Raycaster.enabled = false;

            BackCanvasGroup.DOFade(0, 0.5f).OnComplete(() =>
            {
                BackCanvasGroup.gameObject.SetActive(false);
                Canvas.enabled = false;
            });
        }
        public void getClickid()
        {
            var launchOpt = StarkSDK.API.GetLaunchOptionsSync();
            if (launchOpt.Query != null)
            {
                foreach (KeyValuePair<string, string> kv in launchOpt.Query)
                    if (kv.Value != null)
                    {
                        Debug.Log(kv.Key + "<-参数-> " + kv.Value);
                        if (kv.Key.ToString() == "clickid")
                        {
                            clickid = kv.Value.ToString();
                        }
                    }
                    else
                    {
                        Debug.Log(kv.Key + "<-参数-> " + "null ");
                    }
            }
        }

        public void apiSend(string eventname, string clickid)
        {
            TTRequest.InnerOptions options = new TTRequest.InnerOptions();
            options.Header["content-type"] = "application/json";
            options.Method = "POST";

            JsonData data1 = new JsonData();

            data1["event_type"] = eventname;
            data1["context"] = new JsonData();
            data1["context"]["ad"] = new JsonData();
            data1["context"]["ad"]["callback"] = clickid;

            Debug.Log("<-data1-> " + data1.ToJson());

            options.Data = data1.ToJson();

            TT.Request("https://analytics.oceanengine.com/api/v2/conversion", options,
               response => { Debug.Log(response); },
               response => { Debug.Log(response); });
        }


        /// <summary>
        /// </summary>
        /// <param name="adId"></param>
        /// <param name="closeCallBack"></param>
        /// <param name="errorCallBack"></param>
        public void ShowVideoAd(string adId, System.Action<bool> closeCallBack, System.Action<int, string> errorCallBack)
        {
            starkAdManager = StarkSDK.API.GetStarkAdManager();
            if (starkAdManager != null)
            {
                starkAdManager.ShowVideoAdWithId(adId, closeCallBack, errorCallBack);
            }
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