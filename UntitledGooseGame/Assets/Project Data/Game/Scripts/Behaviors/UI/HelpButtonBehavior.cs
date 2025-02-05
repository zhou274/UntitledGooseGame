#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class HelpButtonBehavior : MonoBehaviour
    {
        private static Dictionary<HelpButtonType, string> names = new Dictionary<HelpButtonType, string>()
        {
            { HelpButtonType.Revert, "Revert" },
            { HelpButtonType.Tip, "Tips" },
            { HelpButtonType.Shuffle, "Shuffle"},
        };

        public static string GetName(HelpButtonType type)
        {
            return names[type];
        }

        [SerializeField] HelpButtonType type;
        [Space]
        [SerializeField] Text amountText;
        [SerializeField] Image itemIcon;
        

        public HelpButtonType Type => type;

        public Sprite itemIconSprite => itemIcon.sprite;

        public void Init(int amount)
        {
            amountText.text = amount.ToString();
        }
    }

    public enum HelpButtonType
    {
        Revert, Tip, Shuffle
    }
}