using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class UIScalableObject
    {
        [SerializeField] float showHideDuration = 0.55f;
        [SerializeField] RectTransform rect;

        public RectTransform RectTransform => rect;
        
        public float ShowHideDuration => showHideDuration;

        public void Show(bool immediately = true, float scaleMultiplier = 1.1f)
        {
            if (immediately)
            {
                rect.localScale = Vector3.one;
                return;
            }

            // RESET
            rect.localScale = Vector3.zero;

            rect.DOPushScale(Vector3.one * scaleMultiplier, Vector3.one, showHideDuration * 0.64f, showHideDuration * 0.36f, Ease.Type.CubicOut, Ease.Type.CubicIn);
        }

        public void Hide(bool immediately = true, float scaleMultiplier = 1.1f, Action onCompleted = null)
        {
            if (immediately)
            {
                rect.localScale = Vector3.zero;
                onCompleted?.Invoke();
                return;
            }

            rect.DOPushScale(Vector3.one * scaleMultiplier, Vector3.zero, showHideDuration * 0.36f, showHideDuration * 0.64f, Ease.Type.CubicOut, Ease.Type.CubicIn).OnComplete(delegate {
                onCompleted?.Invoke();
            });
        }
    }

}