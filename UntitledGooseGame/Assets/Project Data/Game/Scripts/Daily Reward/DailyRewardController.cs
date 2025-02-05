#pragma warning disable 0649

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{

    public class DailyRewardController : MonoBehaviour
    {
        private static DailyRewardController instance;

        [Header("Canvas")]
        [SerializeField] Canvas dailyCanvas;
        [SerializeField] GraphicRaycaster raycaster;

        [Header("Panel")]
        [SerializeField] GameObject signInObject;
        [SerializeField] GameObject signInDoubleObject;
        [SerializeField] GameObject sameDayObject;
        [SerializeField] Image closeButtonImage;

        [Header("Rewards")]
        [SerializeField] List<DailyReward> rewards;

        private static Canvas DailyCanvas => instance.dailyCanvas;
        private static GraphicRaycaster Raycaster => instance.raycaster;

        private static GameObject SignInObject => instance.signInObject;
        private static GameObject SignInDoubleObject => instance.signInDoubleObject;
        private static GameObject SameDayObject => instance.sameDayObject;
        private static Image CloseButtonImage => instance.closeButtonImage;


        private static List<DailyReward> Rewards => instance.rewards;

        private static int DailyDays
        {
            get => PrefsSettings.GetInt(PrefsSettings.Key.DailyDays);
            set => PrefsSettings.SetInt(PrefsSettings.Key.DailyDays, value);
        }

        private static long DailyLastDay
        {
            get => PrefsSettings.GetLong(PrefsSettings.Key.DailyLastDay);
            set => PrefsSettings.SetLong(PrefsSettings.Key.DailyLastDay, value);
        }

        private void Awake()
        {
            
            instance = this;
        }

        public static bool Init()
        {
            DateTime now = DateTime.Now;
            DateTime last = DateTime.FromBinary(DailyLastDay);

            for(int i = 0; i < DailyDays; i++)
            {
                Rewards[i].Init(true);
            }

            for(int i = DailyDays; i < Rewards.Count; i++)
            {
                Rewards[i].Init(false);
            }

            bool watchedAlready = now.Day == last.Day;

            SignInObject.SetActive(!watchedAlready);
            SignInDoubleObject.SetActive(!watchedAlready);
            SameDayObject.SetActive(watchedAlready);

            return !watchedAlready;
        }

        public static void Show()
        {
            DailyCanvas.enabled = true;

            for (int i = 0; i < Rewards.Count; i++)
            {
                DailyReward reward = Rewards[i];

                reward.transform.localScale = Vector3.zero;
            }

            Rewards.DOAction((start, end, t) =>
            {
                float time = t;

                if (time > 1)
                {
                    time = 1 + (time - 1) / 2f;
                }

                Vector3 result = start + (end - start) * t;

                for (int i = 0; i < Rewards.Count; i++)
                {
                    Rewards[i].transform.localScale = result;
                }

                CloseButtonImage.transform.localScale = result;
                SignInObject.transform.localScale = result;
                SignInDoubleObject.transform.localScale = result;
                SameDayObject.transform.localScale = result;

            }, Vector3.zero, Vector3.one, 0.5f).SetEasing(Ease.Type.SineInOut).OnComplete(() => Raycaster.enabled = true);
        }

        public static void Hide()
        {
            Raycaster.enabled = false;

            Rewards.DOAction((start, end, t) => {

                float time = t;

                if (time < 0)
                {
                    time /= 2f;
                }

                Vector3 result = start + (end - start) * t;

                for (int i = 0; i < Rewards.Count; i++)
                {
                    Rewards[i].transform.localScale = result;
                }

                CloseButtonImage.transform.localScale = result;
                SignInObject.transform.localScale = result;
                SignInDoubleObject.transform.localScale = result;
                SameDayObject.transform.localScale = result;

            }, Vector3.one, Vector3.zero, 0.5f).SetEasing(Ease.Type.SineInOut).OnComplete(() => {
                DailyCanvas.enabled = false;
            });


            Tween.DelayedCall(0.5f, () => {
                MainMenuBehavior.Show(true);
            });
        }

        public void SignInButton()
        {
            int reward = Rewards[DailyDays].Reward;

            SignIn(reward);
        }

        private void SignIn(int reward)
        {
            GameController.Coins += reward;

            DailyDays++;

            if (DailyDays == Rewards.Count)
            {
                DailyDays = 0;
            }

            DailyLastDay = DateTime.Now.ToBinary();

            Init();
        }

        public void SignInDoubleButton()
        {
            AdsManager.ShowRewardBasedVideo((watched) =>
            {
                if (watched)
                {
                    int reward = Rewards[DailyDays].Reward * 2;

                    SignIn(reward);
                }
            });
        }

        public void CloseButton()
        {
            Hide();
        }
    }
}