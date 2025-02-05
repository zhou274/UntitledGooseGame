#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    public class GameController : MonoBehaviour
    {
        private static GameController instance;

        [SerializeField] LevelDatabase levelDatabase;
        public static LevelDatabase LevelDatabase => instance.levelDatabase;

        public static int CurrentLevelId
        {
            get => PrefsSettings.GetInt(PrefsSettings.Key.CurrentLevelID);
            set => PrefsSettings.SetInt(PrefsSettings.Key.CurrentLevelID, value);
        }
        public static int ActualLevelId
        {
            get => PrefsSettings.GetInt(PrefsSettings.Key.ActualLevelID);
            set => PrefsSettings.SetInt(PrefsSettings.Key.ActualLevelID, value);
        }
        public static int MaxLevelReachedId
        {
            get => PrefsSettings.GetInt(PrefsSettings.Key.MaxLevelReached);
            set => PrefsSettings.SetInt(PrefsSettings.Key.MaxLevelReached, value);
        }

        public static int Coins
        {
            get => PrefsSettings.GetInt(PrefsSettings.Key.Coins);
            set
            {
                PrefsSettings.SetInt(PrefsSettings.Key.Coins, value);

                MainMenuBehavior.CoinsAmount = value;
            }
        }

        public static int TipsAmount
        {
            get => PrefsSettings.GetInt(PrefsSettings.Key.TipsCount);
            set => PrefsSettings.SetInt(PrefsSettings.Key.TipsCount, value);
        }
        public static int RevertAmount
        {
            get => PrefsSettings.GetInt(PrefsSettings.Key.RevertCount);
            set => PrefsSettings.SetInt(PrefsSettings.Key.RevertCount, value);
        }
        public static int ShuffleAmount
        {
            get => PrefsSettings.GetInt(PrefsSettings.Key.ShuffleCount);
            set => PrefsSettings.SetInt(PrefsSettings.Key.ShuffleCount, value);
        }

        public static bool ShouldOpenDaily { get; set; }

        private void Awake()
        {
            instance = this;

            AdsManager.ShowBanner();
            
            CurrentLevelId = MaxLevelReachedId;
            ActualLevelId = MaxLevelReachedId;
        }

        private void Start()
        {
            LevelController.CreatePools();
            LevelSelectionBehavior.Init();

            MainMenuBehavior.CoinsAmount = Coins;

            ShouldOpenDaily = DailyRewardController.Init();
        }


        public static void LoadLevel(int levelId)
        {
            LoadLevel(LevelDatabase.GetLevel(levelId));
        }

        public static void LoadLevel(Level level)
        {
            GameCanvasBehavior.LevelNumber = CurrentLevelId;

            LevelController.LoadLevel(level);
        }

        public static void Play()
        {
            LoadLevel(MaxLevelReachedId);

            CurrentLevelId = MaxLevelReachedId;
            ActualLevelId = MaxLevelReachedId;

            GameCanvasBehavior.LevelNumber = MaxLevelReachedId;
            GameCanvasBehavior.Show();
        }

        public static void FinishLevel()
        {
            GameCanvasBehavior.RaycasterEnabled = false;

            NextLevel();

        }

        public static void LevelComplete()
        {
            int newMaxReachedLevelId = CurrentLevelId + 1;

            if(newMaxReachedLevelId >= LevelDatabase.AmountOfLevels)
            {
                MaxLevelReachedId = LevelDatabase.AmountOfLevels - 1;
            } 
            else if (newMaxReachedLevelId > MaxLevelReachedId)
            {
                MaxLevelReachedId = newMaxReachedLevelId;
            }
        }

        public static void NextLevel()
        {
            CurrentLevelId++;
            if(CurrentLevelId >= LevelDatabase.AmountOfLevels)
            {
                MaxLevelReachedId = LevelDatabase.AmountOfLevels - 1;

                int newActualLevelId;

                do
                {
                    newActualLevelId = UnityEngine.Random.Range(0, LevelDatabase.AmountOfLevels);
                } while (newActualLevelId == ActualLevelId);

                ActualLevelId = newActualLevelId;

            } else
            {
                if(CurrentLevelId > MaxLevelReachedId)
                {
                    MaxLevelReachedId = CurrentLevelId;
                }
                
                ActualLevelId = CurrentLevelId;
            }

            LoadLevel(ActualLevelId);
        }
    }
}