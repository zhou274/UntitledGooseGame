#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class SlotsController : MonoBehaviour
    {
        private static SlotsController instance;
        
        [SerializeField] List<Image> slots;
        private static List<Image> Slots => instance.slots;

        private static List<MatchableObjectBehavior> objectsInSlots;

        public static bool Filled => Slots.Count == objectsInSlots.Count;

        public static bool IsEmpty => objectsInSlots.Count == 0;

        private static MatchableObjectBehavior lastPickedObject;

        private void Awake()
        {
            instance = this;

            objectsInSlots = new List<MatchableObjectBehavior>();

            lastPickedObject = null;
        }

        public static MatchableObject GetMatchableOfTwo()
        {
            List<MatchableObject> matchables = new List<MatchableObject>();

            for(int i = 0; i < objectsInSlots.Count; i++)
            {
                if (!matchables.Contains(objectsInSlots[i].MatchableObject))
                {
                    matchables.Add(objectsInSlots[i].MatchableObject);
                } else
                {
                    return objectsInSlots[i].MatchableObject;
                }
            }

            return null;
        }

        public static void ReturnThreeLast()
        {
            for(int i = 0; i < 3; i++)
            {
                MatchableObjectBehavior matchable = objectsInSlots[objectsInSlots.Count - 1];

                objectsInSlots.Remove(matchable);

                matchable.transform.DOScale(1, 0.4f);

                LevelController.ReturnMatchable(matchable);
            }

            LevelController.ResetActive();
        }

        public static void Dispose()
        {
            lastPickedObject = null;

            objectsInSlots.DOAction((start, end, t) =>
            {
                for (int i = 0; i < objectsInSlots.Count; i++)
                {
                    objectsInSlots[i].transform.localScale = Vector3.one * (start + (end - start) * t);
                }
            }, 1, 0, 0.4f).SetEasing(Ease.Type.BackIn).OnComplete(() => {
                objectsInSlots.Clear();
            });
        }

        public static void DisposeQuickly()
        {
            lastPickedObject = null;

            for (int i = 0; i < objectsInSlots.Count; i++)
            {
                objectsInSlots[i].transform.localScale = Vector3.zero;
            }

            objectsInSlots.Clear();
        }

        public static MatchableObject GetRandomMatchableInSlots()
        {
            return objectsInSlots.GetRandomItem().MatchableObject;
        }

        public static void DisableRevert()
        {
            lastPickedObject = null;
        }

        public static bool SubmitToSlot(MatchableObjectBehavior matchableObject)
        {
            int index = CalculateIndexSlots(matchableObject);
            
            if(index != objectsInSlots.Count)
            {
                objectsInSlots.Insert(index, matchableObject);
            } 
            else
            {
                objectsInSlots.Add(matchableObject);
            }

            matchableObject.IsTransitioning = true;
            matchableObject.nextTransition = null;

            // Moving object to its slot

            matchableObject.transform.DOMove(Slots[index].transform.position, 0.4f).SetEasing(Ease.Type.SineInOut);
            matchableObject.transform.DOScale(CameraBehavior.ItemInSlotScale, 0.4f);
            matchableObject.transform.DORotate(Vector3.right * 90, 0.4f).SetEasing(Ease.Type.SineInOut).OnComplete(() => 
            {
                matchableObject.IsTransitioning = false;

                matchableObject.nextTransition?.Invoke();

                matchableObject.nextTransition = null;

                if (!CheckMatch(matchableObject) && objectsInSlots.Count == Slots.Count) 
                {
                    for(int i = 0; i < objectsInSlots.Count; i++)
                    {
                        if (objectsInSlots[i].IsTransitioning)
                        {
                            return;
                        }
                    }

                    LevelFailedCanvasBehavior.Show();
                }
            });

            // Shifting objects in slots

            Tween.DelayedCall(0.3f, () => { 
                for(int i = index + 1; i < objectsInSlots.Count; i++)
                {
                    MatchableObjectBehavior nextMatchable = objectsInSlots[i];
                    Image slot = Slots[i];

                    if (nextMatchable.IsTransitioning)
                    {
                        nextMatchable.nextTransition = () => {
                            nextMatchable.transform.DOMove(slot.transform.position, 0.1f).SetEasing(Ease.Type.SineInOut); 
                        };
                    } 
                    else
                    {
                        nextMatchable.transform.DOMove(slot.transform.position, 0.1f).SetEasing(Ease.Type.SineInOut);
                    }
                }
            });

            lastPickedObject = matchableObject;

            return true;
        }

        private static bool CheckMatch(MatchableObjectBehavior matchableObject)
        {
            if (objectsInSlots.IsNullOrEmpty()) return false;

            int counter = 1;
            MatchableObject matchable = objectsInSlots[0].MatchableObject;

            for(int i = 1; i < objectsInSlots.Count; i++)
            {
                if(objectsInSlots[i].MatchableObject == matchable)
                {
                    counter++;

                    if(counter == 3 && matchableObject == objectsInSlots[i])
                    {
                        if (AudioController.IsVibrationEnabled())
                            Vibration.Vibrate(AudioController.Vibrations.shortVibration);

                        RemoveMatch(i - 2);

                        return true;
                    }
                } 
                else
                {
                    counter = 1;
                    matchable = objectsInSlots[i].MatchableObject;
                }
            }

            return false;
        }

        private static void RemoveMatch(int startIndex)
        {
            lastPickedObject = null;

            // Removing objects with animation

            for (int i = startIndex; i <= startIndex + 2; i++)
            {
                MatchableObjectBehavior matchableObject = objectsInSlots[i];
                
                matchableObject.transform.DOScale(0, 0.4f).SetEasing(Ease.Type.SineInOut).OnComplete(() => {
                    matchableObject.transform.localScale = Vector3.one;
                    matchableObject.gameObject.SetActive(false);
                });
            }

            objectsInSlots.RemoveAt(startIndex);
            objectsInSlots.RemoveAt(startIndex);
            objectsInSlots.RemoveAt(startIndex);

            if(GameSettings.CoinsForMatchChance > Random.value)
            {
                GameController.Coins += GameSettings.CoinsForMatch;
            }

            // Shifting the rest of the object

            Tween.DelayedCall(0.3f, () => {
                for (int i = 0; i < objectsInSlots.Count; i++)
                {
                    MatchableObjectBehavior nextMatchable = objectsInSlots[i];
                    Image slot = Slots[i];

                    if (nextMatchable.IsTransitioning)
                    {
                        nextMatchable.nextTransition = () => {
                            nextMatchable.transform.DOMove(slot.transform.position, 0.1f).SetEasing(Ease.Type.SineInOut);
                        };
                    }
                    else
                    {
                        nextMatchable.transform.DOMove(slot.transform.position, 0.1f).SetEasing(Ease.Type.SineInOut);
                    }
                }
            });

            AudioController.PlaySound(AudioController.Sounds.mergeSound);

            LevelController.CheckLevelFinish();
        }

        public static int AmountInSlots(MatchableObject matchable)
        {
            return objectsInSlots.FindAll((MatchableObjectBehavior matchableObject) => { return matchableObject.MatchableObject == matchable; }).Count;
        }

        private static int CalculateIndexSlots(MatchableObjectBehavior matchableObject)
        {
            for(int i = 0; i < objectsInSlots.Count; i++)
            {
                if(matchableObject.MatchableObject == objectsInSlots[i].MatchableObject)
                {
                    for(int j = i + 1; j < objectsInSlots.Count; j++)
                    {
                        if(matchableObject.MatchableObject != objectsInSlots[j].MatchableObject)
                        {
                            return j;
                        }
                    }
                }
            }

            return objectsInSlots.Count;
        }

        public static int GetSlotsAvailable()
        {
            return Slots.Count - objectsInSlots.Count;
        }

        public static bool Revert()
        {
            if (lastPickedObject == null) return false;

            // Returing last object to the level field

            LevelController.PlaceMatchable(lastPickedObject, true);
            LevelController.AddMatchableToRepresentation(lastPickedObject);
            LevelController.ResetActive();
            lastPickedObject.transform.DOScale(1, 0.4f);

            objectsInSlots.Remove(lastPickedObject);

            lastPickedObject = null;

            // Shifting the rest of the objects

            Tween.DelayedCall(0.3f, () => {
                for (int i = 0; i < objectsInSlots.Count; i++)
                {
                    MatchableObjectBehavior nextMatchable = objectsInSlots[i];
                    Image slot = Slots[i];

                    if (nextMatchable.IsTransitioning)
                    {
                        nextMatchable.nextTransition = () => {
                            nextMatchable.transform.DOMove(slot.transform.position, 0.1f).SetEasing(Ease.Type.SineInOut);
                        };
                    }
                    else
                    {
                        nextMatchable.transform.DOMove(slot.transform.position, 0.1f).SetEasing(Ease.Type.SineInOut);
                    }
                }

                Tween.DelayedCall(0.1f, () => GameCanvasBehavior.RaycasterEnabled = true);
            });

            return true;
        }
    }
}