#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Watermelon
{

    public class StoreCanvasBehavior : MonoBehaviour
    {
        private static StoreCanvasBehavior instance;

        [Header("Canvas")]
        [SerializeField] Canvas canvas;
        [SerializeField] RectTransform backPanel;
        [SerializeField] CanvasGroup backCanvasGroup;
        [SerializeField] GraphicRaycaster raycaster;

        private static Canvas Canvas => instance.canvas;
        private static RectTransform BackPanel => instance.backPanel;
        private static CanvasGroup BackCanvasGroup => instance.backCanvasGroup;
        private static GraphicRaycaster Raycaster => instance.raycaster;

        [Header("Coins Panel")]
        [SerializeField] RectTransform coinsPanel;
        [SerializeField] Text coinsAmount;

        private static RectTransform CoinsPanel => instance.coinsPanel;
        private static Text CoinsAmount => instance.coinsAmount;

        [Header("Store Info")]
        [SerializeField] Text itemName;
        [SerializeField] Image itemIcon;
        [SerializeField] Text itemsAmount;
        [SerializeField] Text costText;

        private static Text ItemName => instance.itemName;
        private static Image ItemIcon => instance.itemIcon;
        private static Text ItemAmountText => instance.itemsAmount;
        private static Text CostText => instance.costText;

        private static int ItemAmount { get; set; }
        private static int CostOfOneItem { get; set; }

        private static HelpButtonType Type { get; set; }

        private void Awake()
        {
            instance = this;
        }

        public void IncreaseAmount()
        {
            ItemAmount++;
            ItemAmountText.text = ItemAmount.ToString();

            CostText.text = ": " + CostOfOneItem * ItemAmount + "/" + GameController.Coins;
        }

        public void DecreaseAmount()
        {
            ItemAmount--;
            if (ItemAmount == 0) ItemAmount = 1;
            ItemAmountText.text = ItemAmount.ToString();

            CostText.text = ": " + CostOfOneItem * ItemAmount + "/" + GameController.Coins;
        }

        public void PurchaseButton()
        {
            if(GameController.Coins > CostOfOneItem * ItemAmount)
            {
                switch (Type)
                {
                    case HelpButtonType.Revert:
                        GameController.RevertAmount += ItemAmount;
                        break;
                    case HelpButtonType.Shuffle:
                        GameController.ShuffleAmount += ItemAmount;
                        break;
                    case HelpButtonType.Tip:
                        GameController.TipsAmount += ItemAmount;
                        break;
                }

                GameController.Coins -= CostOfOneItem * ItemAmount;

                CoinsAmount.text = GameController.Coins.ToString();

                CostText.text = ": " + CostOfOneItem * ItemAmount + "/" + GameController.Coins;

                GameCanvasBehavior.InitButtons();
            }
        }

        public void GetForAdButton()
        {
            AdsManager.ShowRewardBasedVideo((finished) => {
                if (finished)
                {
                    switch (Type)
                    {
                        case HelpButtonType.Revert:
                            GameController.RevertAmount++;
                            break;
                        case HelpButtonType.Shuffle:
                            GameController.ShuffleAmount++;
                            break;
                        case HelpButtonType.Tip:
                            GameController.TipsAmount++;
                            break;
                    }

                    CostText.text = ": " + CostOfOneItem * ItemAmount + "/" + GameController.Coins;

                    GameCanvasBehavior.InitButtons();
                }
            });
        }

        public void CloseButton()
        {
            BackPanel.DOScale(0, 0.5f).SetEasing(Ease.Type.SineInOut);
            CoinsPanel.transform.DOScale(0, 0.5f).SetEasing(Ease.Type.SineInOut);
            BackCanvasGroup.DOFade(0, 0.5f).OnComplete(() => {
                BackCanvasGroup.gameObject.SetActive(false);
                Canvas.enabled = false;

                GameCanvasBehavior.RaycasterEnabled = true;
            });

            AdsManager.HideBanner();
            Raycaster.enabled = false;
        }

        public static void Show(HelpButtonBehavior helpButton)
        {
            Canvas.enabled = true;
            BackCanvasGroup.gameObject.SetActive(true);

            BackPanel.localScale = Vector3.zero;
            CoinsPanel.transform.localScale = Vector3.zero;
            BackCanvasGroup.alpha = 0;

            BackPanel.DOScale(1, 0.5f).SetEasing(Ease.Type.SineInOut);
            CoinsPanel.transform.DOScale(1, 0.5f).SetEasing(Ease.Type.SineInOut);
            BackCanvasGroup.DOFade(1f, 0.5f).OnComplete(() => {
                AdsManager.ShowBanner();

                Raycaster.enabled = true;
            });

            CoinsAmount.text = GameController.Coins.ToString();

            Type = helpButton.Type;

            ItemName.text = HelpButtonBehavior.GetName(Type);
            ItemIcon.sprite = helpButton.itemIconSprite;
            ItemAmountText.text = "1";

            ItemAmount = 1;

            CostOfOneItem = GameSettings.GetHelpButtonCost(Type);

            CostText.text = ": " + CostOfOneItem + "/" + GameController.Coins;
        }
    }
}