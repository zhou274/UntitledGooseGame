#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;
using StarkSDKSpace;


namespace Watermelon
{

    public class GameCanvasBehavior : MonoBehaviour
    {
        private static GameCanvasBehavior instance;

        [Header("Canvases")]
        [SerializeField] Canvas gameCanvas;
        [SerializeField] Canvas slotsCanvas;

        [Space]
        [SerializeField] CanvasGroup gameCanvasGroup;
        [SerializeField] CanvasGroup slotsCanvasGroup;

        [Space]
        [SerializeField] GraphicRaycaster raycaster;

        [Header("Game Panel")]
        [SerializeField] TextMeshProUGUI levelNumber;
        [SerializeField] RectTransform pauseButton;

        [Header("Slots")]
        [SerializeField] RectTransform slotsTransform;

        [Header("Help Buttons")]
        [SerializeField] RectTransform buttonsTransform;

        [Space]
        [SerializeField] HelpButtonBehavior tipButton;
        [SerializeField] HelpButtonBehavior revertButton;
        [SerializeField] HelpButtonBehavior shuffleButton;

        

        private static RectTransform ButtonsTransform => instance.buttonsTransform;
        private static RectTransform PauseButton => instance.pauseButton;
        private static RectTransform LevelNumberTransform => instance.levelNumber.rectTransform;

        private static RectTransform SlotsTransform => instance.slotsTransform;

        private static Canvas GameCanvas => instance.gameCanvas;
        private static Canvas SlotsCavas => instance.slotsCanvas;

        public static bool RaycasterEnabled { get => instance.raycaster.enabled; set => instance.raycaster.enabled = value; }

        private static CanvasGroup GameCanvasGroup => instance.gameCanvasGroup;
        private static CanvasGroup SlotsCanvasGroup => instance.slotsCanvasGroup;

        public static int LevelNumber { set => instance.levelNumber.text = "关卡 " + (value + 1); }
        public string clickid;
        private StarkAdManager starkAdManager;

        private void Awake()
        {
            instance = this;
        }

        public static void Show()
        {
            GameCanvas.enabled = true;
            SlotsCavas.enabled = true;

            GameCanvasGroup.alpha = 0;
            SlotsCanvasGroup.alpha = 0;

            GameCanvasGroup.DOFade(1, 0.4f);
            SlotsCanvasGroup.DOFade(1, 0.4f);

            if (MainMenuBehavior.IsTablet())
            {
                SlotsTransform.anchoredPosition = new Vector2(0, 300);
            } 
            else
            {
                SlotsTransform.anchoredPosition = new Vector2(0, 400);
            }

            ButtonsTransform.anchoredPosition = new Vector2(0, -300);
            PauseButton.anchoredPosition = new Vector2(-120, 100);
            LevelNumberTransform.anchoredPosition = new Vector2(0, 300); 

            ButtonsTransform.DOAnchoredPosition(Vector2.zero, 0.4f).SetEasing(Ease.Type.SineOut);
            PauseButton.DOAnchoredPosition(new Vector2(-120, -110), 0.4f).SetEasing(Ease.Type.SineOut);
            LevelNumberTransform.DOAnchoredPosition(new Vector2(0, -100), 0.4f).SetEasing(Ease.Type.SineOut);

            instance.tipButton.Init(GameController.TipsAmount);
            instance.revertButton.Init(GameController.RevertAmount);
            instance.shuffleButton.Init(GameController.ShuffleAmount);
        }

        public static void Hide()
        {
            GameCanvasGroup.alpha = 1;
            SlotsCanvasGroup.alpha = 1;

            GameCanvasGroup.DOFade(0, 0.4f);
            SlotsCanvasGroup.DOFade(0, 0.4f).OnComplete(() => {
                GameCanvas.enabled = false;
            });

            RaycasterEnabled = false;
        }

        public static void HideInstantly()
        {
            GameCanvasGroup.alpha = 1;
            SlotsCanvasGroup.alpha = 1;

            GameCanvasGroup.DOFade(0, 0.4f);
            SlotsCanvasGroup.DOFade(0, 0.4f).OnComplete(() => {
                GameCanvas.enabled = false;
            });

            RaycasterEnabled = false;
        }

        public void ShuffleButton()
        {
            ShowVideoAd("192if3b93qo6991ed0",
            (bol) => {
                if (bol)
                {

                    RaycasterEnabled = false;
                    LevelController.Shuffle();
                    GameController.ShuffleAmount--;

                    instance.shuffleButton.Init(GameController.ShuffleAmount);


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
            
            //if (GameController.ShuffleAmount > 0)
            //{
            //    LevelController.Shuffle();
            //    GameController.ShuffleAmount--;

            //    instance.shuffleButton.Init(GameController.ShuffleAmount);
            //} else
            //{
            //    StoreCanvasBehavior.Show(shuffleButton);
            //}
        }

        public void RevertButton()
        {
            ShowVideoAd("192if3b93qo6991ed0",
            (bol) => {
                if (bol)
                {
                    RaycasterEnabled = false;
                    if (SlotsController.Revert())
                    {
                        GameController.RevertAmount--;

                        instance.revertButton.Init(GameController.RevertAmount);
                    }
                    else
                    {
                        RaycasterEnabled = true;
                    }



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
            
            //if (GameController.RevertAmount > 0)
            //{
            //    if (SlotsController.Revert()) {
            //        GameController.RevertAmount--;

            //        instance.revertButton.Init(GameController.RevertAmount);
            //    } else
            //    {
            //        RaycasterEnabled = true;
            //    }
            //} else
            //{
            //    StoreCanvasBehavior.Show(revertButton);
            //}
        }

        public void TipButton()
        {
            ShowVideoAd("192if3b93qo6991ed0",
            (bol) => {
                if (bol)
                {
                    RaycasterEnabled = false;
                    LevelController.RemoveAsTip();

                    GameController.TipsAmount--;

                    instance.tipButton.Init(GameController.TipsAmount);



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
            
            //if (GameController.TipsAmount > 0)
            //{

            //    LevelController.RemoveAsTip();

            //    GameController.TipsAmount--;

            //    instance.tipButton.Init(GameController.TipsAmount);
            //} else
            //{
            //    StoreCanvasBehavior.Show(tipButton);
            //}
        }

        public void PauseButon()
        {
            RaycasterEnabled = false;
            GamePauseCanvasBehavior.Show();
        } 

        public static void InitButtons()
        {
            instance.tipButton.Init(GameController.TipsAmount);
            instance.revertButton.Init(GameController.RevertAmount);
            instance.shuffleButton.Init(GameController.ShuffleAmount);
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
    }
}
