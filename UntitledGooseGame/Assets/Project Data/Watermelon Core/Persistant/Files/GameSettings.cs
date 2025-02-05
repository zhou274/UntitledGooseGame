using UnityEngine;

namespace Watermelon
{
    [SetupTab("Settings", texture = "icon_settings", priority = 0)]
    [CreateAssetMenu(fileName = "Game Settings", menuName = "Settings/Game Settings")]
    public class GameSettings : ScriptableObject, IInitialized
    {
        private static GameSettings instance;

        [Header("Coins")]
        [SerializeField] int coinsForMatch;
        [SerializeField] float coinsForMatchChance;
        [Space]
        [SerializeField] int coinsForAd;

        [Space]
        [SerializeField] int revertCost;
        [SerializeField] int tipCost;
        [SerializeField] int shuffleCost;

        public static int CoinsForMatch => instance.coinsForMatch;
        public static float CoinsForMatchChance => instance.coinsForMatchChance;

        public static int CoinsForAd => instance.coinsForAd;

        public void Init()
        {
            instance = this;
        }

        public static int GetHelpButtonCost(HelpButtonType type)
        {
            switch (type)
            {
                case HelpButtonType.Revert:
                    return instance.revertCost;

                case HelpButtonType.Shuffle:
                    return instance.shuffleCost;

                case HelpButtonType.Tip:
                    return instance.tipCost;
            }

            return 0;
        }
    }
}
