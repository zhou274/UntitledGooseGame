#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class LevelCellBehavior : MonoBehaviour
    {
        [SerializeField] Text levelNumber;
        [SerializeField] Image currentLevelIndicator;
        [SerializeField] Image lockImage;

        [Space]
        [SerializeField] Button button;

        private int levelId;
        public int LevelNumber {
            get => levelId;
            set {
                levelNumber.text = (value + 1).ToString();
                levelId = value;
            }
        }

        public bool IsSelected { set => currentLevelIndicator.enabled = value; }

        public bool IsOpened { 
            set
            {
                lockImage.enabled = !value;
                levelNumber.enabled = value;

                button.enabled = value;
            } 
        }

        public void OnClick()
        {
            LevelSelectionBehavior.Hide();

            GameController.CurrentLevelId = LevelNumber;
            GameController.ActualLevelId = LevelNumber;

            Tween.DelayedCall(0.4f, () => { 
                GameController.LoadLevel(LevelNumber);
                GameCanvasBehavior.LevelNumber = GameController.CurrentLevelId;
                GameCanvasBehavior.Show();
            });
        }
    }
}

