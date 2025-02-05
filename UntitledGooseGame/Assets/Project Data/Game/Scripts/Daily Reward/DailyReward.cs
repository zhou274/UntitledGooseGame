#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon{

    public class DailyReward : MonoBehaviour
    {
        [SerializeField] int reward;

        [Space]
        [SerializeField] GameObject uncollectedInfo;
        [SerializeField] GameObject collectedInfo;

        [Space]
        [SerializeField] Text rewardAmount;

        public int Reward => reward;

        public void Init(bool collected)
        {
            uncollectedInfo.SetActive(!collected);
            collectedInfo.SetActive(collected);

            rewardAmount.text = reward.ToString();
        }
    }
}