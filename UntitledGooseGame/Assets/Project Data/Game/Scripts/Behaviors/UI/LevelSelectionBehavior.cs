#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Watermelon
{
    public class LevelSelectionBehavior : MonoBehaviour
    {
        private static readonly string LEVEL_CELL_POOL_NAME = "Level Cell";

        private static LevelSelectionBehavior instance;

        [Header("Canvas")]
        [SerializeField] Canvas levelSelectionCanvas;
        [SerializeField] CanvasGroup levelSelectionCanvasGroup;
        [SerializeField] GraphicRaycaster raycaster;
        
        [Header("Panel")]
        [SerializeField] RectTransform scrollViewRect;
        [SerializeField] RectTransform contentRect;
        [SerializeField] RectTransform gridRect;

        private static Canvas LevelSelectionCanvas => instance.levelSelectionCanvas;
        private static CanvasGroup LevelSelectionCanvasGroup => instance.levelSelectionCanvasGroup;
        private static GraphicRaycaster Raycaster => instance.raycaster;

        private static RectTransform ContentRect => instance.contentRect;
        private static RectTransform GridRect => instance.gridRect;

        private static Pool levelCellPool;

        private void Awake()
        {
            instance = this;

            levelCellPool = PoolManager.GetPoolByName(LEVEL_CELL_POOL_NAME);
        }

        public static void Init()
        {
            levelCellPool.ReturnToPoolEverything();
            
            for(int i = 0; i < GameController.LevelDatabase.AmountOfLevels; i++)
            {
                LevelCellBehavior level = levelCellPool.GetPooledObject().GetComponent<LevelCellBehavior>();

                level.transform.SetParent(GridRect.transform);
                level.transform.localScale = Vector3.one;
                level.transform.localRotation = Quaternion.identity;

                level.LevelNumber = i;
                level.IsSelected = i == GameController.CurrentLevelId;
                level.IsOpened = i <= GameController.MaxLevelReachedId;
            }

            ContentRect.sizeDelta = ContentRect.sizeDelta.SetY((GameController.LevelDatabase.AmountOfLevels / 4 + 1) * 240);
        }

        public static void Show()
        {
            Init();

            LevelSelectionCanvas.enabled = true;

            LevelSelectionCanvasGroup.alpha = 0;

            LevelSelectionCanvasGroup.DOFade(1, 0.4f).OnComplete(() => Raycaster.enabled = true);
        }

        public static void Hide()
        {
            LevelSelectionCanvasGroup.DOFade(0, 0.4f).OnComplete(() => {
                LevelSelectionCanvas.enabled = false;
            });

            Raycaster.enabled = false;

            AdsManager.HideBanner();
        }

        public void BackButton()
        {
            Hide();
            Tween.DelayedCall(0.4f, () => MainMenuBehavior.Show(true));
        }
    }
}
