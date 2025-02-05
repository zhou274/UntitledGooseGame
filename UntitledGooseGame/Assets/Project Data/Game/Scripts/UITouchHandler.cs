#pragma warning disable 0414

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Watermelon
{
    public class UITouchHandler : MonoBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            if(Physics.Raycast(CameraBehavior.MainCamera.ScreenPointToRay(eventData.position), out RaycastHit hit, 20, 256))
            {
                MatchableObjectBehavior matchable = hit.transform.parent.GetComponent<MatchableObjectBehavior>();

                if (matchable.IsActive && !SlotsController.Filled)
                {
                    LevelController.RemoveMatchable(matchable);

                    AudioController.PlaySound(AudioController.Sounds.itemClick);
                } 
                else
                {
                    matchable.Shake();
                }
            }
        }
    }
}