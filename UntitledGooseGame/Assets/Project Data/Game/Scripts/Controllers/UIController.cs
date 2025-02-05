#pragma warning disable 0649

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

//UI Module v0.9.1
public class UIController : MonoBehaviour
{
    private static UIController instance;
    public static UIController Instance { get { return instance; } }

    [Header("Settings")]
    [SerializeField] float noThanksAppearDelay;

    [Header("Menu Panel")]
    [LineSpacer("References")]
    [SerializeField] GameObject mainMenuPanel;

    [Header("Game Panel")]
    [SerializeField] Image levelProgressFillImage;
    [SerializeField] Text currentLevelText;
    [SerializeField] Text nextLevelText;

    [Header("Level Complete Panel")]
    [SerializeField] GameObject levelCompletedPanel;
    [SerializeField] Text tapToContinueText;
    [SerializeField] RectTransform levelCompletedCongradsTextRect;

    [Header("Game Over Panel")]
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] GameObject noThanksButtonObject;
    [SerializeField] Text noThanksText;
    [SerializeField] Image gameOverFadeImage;

    [Header("Other")]
    [SerializeField] CanvasScaler canvasScalerRef;
    [SerializeField] Text popupText;

    [Header("Developement")]
    [SerializeField] GameObject devPanel;

    private TweenCase popUpTween;
    private TweenCase noThanksButtonTween;
    private RectTransform popUpRect;

    private float gameOverFadeImageAlpha;

    private void Awake()
    {
        instance = this;
        popUpRect = popupText.rectTransform;
        gameOverFadeImageAlpha = gameOverFadeImage.color.a;

        SetupUIForScreenRatio();
    }

    private void SetupUIForScreenRatio()
    {
        float screenRatio = Screen.width / (float)Screen.height;

        if (screenRatio > canvasScalerRef.referenceResolution.x / canvasScalerRef.referenceResolution.y)
        {
            canvasScalerRef.matchWidthOrHeight = 1f;
        }
    }

    public void UpdateLevelProgress(float progress)
    {
        levelProgressFillImage.fillAmount = progress;
    }

    public void UpdateLevelsText(int currentLevel)
    {
        currentLevelText.text = currentLevel.ToString();
        nextLevelText.text = (currentLevel + 1).ToString();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        levelCompletedPanel.SetActive(false);
    }

    public void HideMainMenu()
    {
        mainMenuPanel.SetActive(false);
    }

    public void ShowGameOverPanel()
    {
        gameOverFadeImage.gameObject.SetActive(true);
        gameOverFadeImage.color = gameOverFadeImage.color.SetAlpha(0f);
        gameOverFadeImage.DOFade(gameOverFadeImageAlpha, 0.1f).SetEasing(Ease.Type.CubicIn);

        noThanksButtonObject.SetActive(false);
        noThanksButtonTween = Tween.DelayedCall(noThanksAppearDelay, () =>
        {
            noThanksButtonObject.SetActive(true);
            noThanksText.color = Color.white.SetAlpha(0f);
            noThanksButtonTween = noThanksText.DOColor(Color.white, 0.3f).SetEasing(Ease.Type.CubicOut);
        });

        gameOverPanel.SetActive(true);
    }

    public void HideGameOverPanel()
    {
        if (noThanksButtonTween != null)
        {
            noThanksButtonTween.Kill();
        }

        gameOverFadeImage.gameObject.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void ShowLevelCompletedPanel()
    {
        levelCompletedPanel.SetActive(true);

        tapToContinueText.color = tapToContinueText.color.SetAlpha(0f);

        levelCompletedCongradsTextRect.localScale = Vector3.zero;
        levelCompletedCongradsTextRect.DOScale(Vector3.one * 1.2f, 0.8f).SetEasing(Ease.Type.CubicIn).OnComplete(() =>
        {
            levelCompletedCongradsTextRect.DOScale(Vector3.one, 0.4f).SetEasing(Ease.Type.CubicOut).OnComplete(() =>
            {
                tapToContinueText.DOColor(tapToContinueText.color.SetAlpha(1f), 0.5f).SetEasing(Ease.Type.QuadIn);
            });
        });
    }

    #region Buttons

    public void ReviveButton()
    {
        Debug.Log("[UI Module] Revive button pressed");
    }

    public void NoThanksButton()
    {
        Debug.Log("[UI Module] No thanks button pressed");
    }

    public void TapToContinueButton()
    {
        Debug.Log("[UI Module] Continue button pressed");
    }

    #endregion

    #region Popups

    public void ShowPositivePopUpText()
    {
        if (popUpTween != null && popUpTween.isActive)
        {
            popUpTween.Kill();
        }

        popupText.text = "POP UP TEXT";

        popupText.gameObject.SetActive(true);
        popupText.color = popupText.color.SetAlpha(1f);
        popupText.rectTransform.anchoredPosition = new Vector3(UnityEngine.Random.Range(-50f, 50f), UnityEngine.Random.Range(-50f, 50f), 0f);
        popupText.rectTransform.eulerAngles = new Vector3(0, 0, UnityEngine.Random.Range(-20f, 20f));
        popupText.rectTransform.localScale = Vector3.zero;

        popUpTween = popupText.rectTransform.DOScale(1.5f, 0.2f).OnComplete(delegate
        {
            popUpTween = popupText.rectTransform.DOScale(1, 0.1f).OnComplete(delegate
             {
                 popUpTween = Tween.DelayedCall(1, delegate
                 {
                     popUpTween = popupText.DOFade(0, 0.2f).OnComplete(delegate
                     {
                         popupText.gameObject.SetActive(false);
                     });
                 });
             });
        });
    }

    public void ShowPositivePopUpTextAlternative()
    {
        if (popUpTween != null && popUpTween.isActive)
        {
            popUpTween.Kill();
        }

        popupText.text = "POP UP TEXT";

        popupText.color = popupText.color.SetAlpha(1f);
        popUpRect.anchoredPosition = new Vector3(UnityEngine.Random.Range(-50f, 50f), -240f, 0f);

        popUpTween = Tween.DoFloat(-220f, 0f, 0.5f, (float val) => popUpRect.anchoredPosition = popUpRect.anchoredPosition.SetY(val)).SetEasing(Ease.Type.CubicOut).OnComplete(delegate
       {
           popUpTween = popupText.DOFade(0, 0.2f);
       });
    }

    public void ShowNegativePopUpText()
    {
        if (popUpTween != null && popUpTween.isActive)
        {
            popUpTween.Kill();
        }

        popupText.text = "POP UP TEXT";

        popupText.color = popupText.color.SetAlpha(1f);
        popUpRect.anchoredPosition = new Vector3(UnityEngine.Random.Range(-50f, 50f), UnityEngine.Random.Range(150f, 200f), 0f);
        popUpRect.localScale = Vector3.one;

        popUpTween = popUpRect.DOLocalMoveY(0f, 0.2f).SetEasing(Ease.Type.BounceOut).OnComplete(delegate
        {
            popUpTween = Tween.DelayedCall(1, delegate
            {
                popUpTween = popupText.DOFade(0, 0.2f);
            });
        });
    }

    #endregion

    #region Dev

    public void FirstLevelDevButton()
    {
        Debug.Log("[UI Module] Dev button pressed");
    }

    public void PrevLevelDevButton()
    {
        Debug.Log("[UI Module] Dev button pressed");
    }

    public void NextLevelDevButton()
    {
        Debug.Log("[UI Module] Dev button pressed");
    }

    public void ColorDevButton()
    {
        Debug.Log("[UI Module] Dev button pressed");
    }

    public void HideDevButton()
    {
        devPanel.SetActive(false);
    }

    #endregion
}