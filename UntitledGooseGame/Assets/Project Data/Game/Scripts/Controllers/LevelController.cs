using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class LevelController : MonoBehaviour
    {
        private static LevelController instance;
        private static Dictionary<MatchableObject, Pool> matchableObjectsPools;

        public static Level Level { get; private set; }

        public static LevelRepresentation LevelRepresentation { get; private set; }

        private static List<MatchableObjectBehavior> matchableObjects;

        public static int LevelEvenSize => Level.FirstLayerSize;
        public static int LevelOddSize => Level.FirstLayerSize + 1;

        private void Awake()
        {
            instance = this;

            LevelRepresentation = new LevelRepresentation();
            matchableObjects = new List<MatchableObjectBehavior>();
        }

        public static void CreatePools()
        {
            if (matchableObjectsPools == null) matchableObjectsPools = new Dictionary<MatchableObject, Pool>();

            matchableObjectsPools.Clear();

            for (int i = 0; i < GameController.LevelDatabase.AmountOfMatchableObjects; i++)
            {

                MatchableObject matchableObject = GameController.LevelDatabase.GetmatchableObject(i);

                Pool pool = PoolManager.AddPool(new PoolSettings
                {
                    name = matchableObject.Prefab.name + ":" + i,
                    autoSizeIncrement = true,
                    type = Pool.PoolType.Single,
                    objectsContainer = null,
                    size = 12,
                    singlePoolPrefab = matchableObject.Prefab
                });

                matchableObjectsPools.Add(matchableObject, pool);
            }
        }

        public static void ResetActive()
        {
            for (int i = 0; i < matchableObjects.Count; i++)
            {
                matchableObjects[i].SetActive(LevelRepresentation.IsAvailable(matchableObjects[i]));
            }
        }

        public static void ReturnMatchable(MatchableObjectBehavior matchable)
        {
            matchableObjects.Add(matchable);

            AddMatchableToRepresentation(matchable);

            PlaceMatchable(matchable, true);
        }

        public static void LoadLevel(Level level)
        {
            Level = level;

            CameraBehavior.Init(LevelOddSize);

            // Preparing objects to be placed on the level

            List<MatchableObject> availableObjects = GameController.LevelDatabase.AvailableForLevel(Level);

            List<MatchableObjectBehavior> levelPool = new List<MatchableObjectBehavior>();
            Dictionary<MatchableObject, int> objectsInLevelAmount = new Dictionary<MatchableObject, int>();

            int objectsLeft = Level.GetAmountOfFilledCells();

            int max = 1;

            while(objectsLeft > 0)
            {
                MatchableObject matchable = availableObjects.GetRandomItem();

                if (objectsInLevelAmount.ContainsKey(matchable))
                {
                    for(int i = 0; i < availableObjects.Count; i++)
                    {
                        MatchableObject testMatchable = availableObjects[i];
                        if (testMatchable != matchable)
                        {
                            if (objectsInLevelAmount.ContainsKey(testMatchable))
                            {
                                if(objectsInLevelAmount[testMatchable] < max)
                                {
                                    matchable = testMatchable;
                                }
                            } else
                            {
                                matchable = testMatchable;
                                objectsInLevelAmount.Add(matchable, 1);
                            }
                            
                        }
                    }

                    int amount = objectsInLevelAmount[matchable];
                    amount++;

                    if (max < amount) max = amount;
                    objectsInLevelAmount[matchable] = amount;
                } 
                else
                {
                    objectsInLevelAmount.Add(matchable, 1);
                    if (max == 0) max = 1;
                }

                for(int i = 0; i < 3; i++)
                {
                    MatchableObjectBehavior mathableObject = matchableObjectsPools[matchable].GetPooledObject().GetComponent<MatchableObjectBehavior>();

                    mathableObject.MatchableObject = matchable;

                    mathableObject.transform.position = new Vector3(-2, -2, -2);

                    levelPool.Add(mathableObject);
                }
                
                objectsLeft -= 3;
            }

            // Initing level representation

            LevelRepresentation.Init(Level);

            matchableObjects.Clear();

            // Spawning objects

            instance.StartCoroutine(SpawnLevelCoroutine(levelPool));
        }

        private static IEnumerator SpawnLevelCoroutine(List<MatchableObjectBehavior> levelPool)
        {
            for (int i = Level.AmountOfLayers - 1; i >= 0; i--)
            {
                Layer layer = Level.GetLayer(i);

                bool isEven = i % 2 == 0;

                int size = isEven ? LevelEvenSize : LevelOddSize; ;

                List<MatchableObjectBehavior> layerMatchables = new List<MatchableObjectBehavior>();

                for (int y = size - 1; y >= 0 ; y--)
                {
                    bool placed = false;
                    for (int x = 0; x < size; x++)
                    {
                        // Initializing object if there is one

                        if (layer[y][x])
                        {
                            MatchableObjectBehavior matchable = levelPool.GetRandomItem();

                            levelPool.Remove(matchable);

                            matchable.Init(i, x, y);

                            PlaceMatchable(matchable);

                            layerMatchables.Add(matchable);

                            matchable.transform.localScale = Vector3.zero;
                            matchable.transform.DOScale(1, 0.4f).SetEasing(Ease.Type.BackOut);

                            placed = true;

                            matchable.SetActive(true, false);
                        }
                    }
                    if(placed) yield return new WaitForSeconds(0.07f);
                }

                // Figuring out is object is Active

                for (int j = 0; j < layerMatchables.Count; j++)
                {
                    layerMatchables[j].SetActive(LevelRepresentation.IsAvailable(layerMatchables[j]));
                }

                yield return new WaitForSeconds(0.2f);

                matchableObjects.AddRange(layerMatchables);
            }

            Tween.DelayedCall(0.2f, () => GameCanvasBehavior.RaycasterEnabled = true);
        }

        public static void AddMatchableToRepresentation(MatchableObjectBehavior matchableObject)
        {
            LevelRepresentation.layers[matchableObject.LayerId].grid[matchableObject.Y, matchableObject.X] = true;
        }

        public static void PlaceMatchable(MatchableObjectBehavior matchable, bool withAnimation = false)
        {
            Vector3 position;

            if (matchable.LayerId % 2 == 0)
            {
                position = new Vector3(CameraBehavior.HalfWidth - LevelEvenSize / 2f + matchable.X + 0.5f, 5 - matchable.LayerId, CameraBehavior.MainCamera.orthographicSize - LevelOddSize / 2f + 1.5f + matchable.Y);
            }
            else
            {
                position = new Vector3(CameraBehavior.HalfWidth - LevelOddSize / 2f + matchable.X + 0.5f, 5 - matchable.LayerId, CameraBehavior.MainCamera.orthographicSize - LevelOddSize / 2f + 1f + matchable.Y);
            }

            Vector3 rotation = new Vector3(Random.value * 360, Random.value * 360, Random.value * 360);

            if (withAnimation)
            {
                matchable.transform.DOMove(position, 0.4f).SetEasing(Ease.Type.SineInOut);
                matchable.transform.DORotate(rotation, 0.4f).SetEasing(Ease.Type.SineInOut);
            } else
            {
                matchable.transform.position = position;
                matchable.transform.eulerAngles = rotation;
            }
        }

        public static void RemoveMatchable(MatchableObjectBehavior matchable)
        {
            //matchable.gameObject.SetActive(false);
            SlotsController.SubmitToSlot(matchable);

            LevelRepresentation.layers[matchable.LayerId].grid[matchable.Y, matchable.X] = false;

            matchableObjects.Remove(matchable);

            for(int i = 0; i < matchableObjects.Count; i++)
            {
                matchableObjects[i].SetActive(LevelRepresentation.IsAvailable(matchableObjects[i]));
            }
        }

        public static void RemoveMatchables(params MatchableObjectBehavior[] matchables)
        {
            for(int i = 0; i < matchables.Length; i++)
            {
                MatchableObjectBehavior matchable = matchables[i];

                SlotsController.SubmitToSlot(matchable);

                LevelRepresentation.layers[matchable.LayerId].grid[matchable.Y, matchable.X] = false;

                matchableObjects.Remove(matchable);
            }

            for (int i = 0; i < matchableObjects.Count; i++)
            {
                matchableObjects[i].SetActive(LevelRepresentation.IsAvailable(matchableObjects[i]));
            }
        }

        public static void Shuffle()
        {
            matchableObjects.Shuffle();

            instance.StartCoroutine(ShuffleCoroutine());
        }

        private static IEnumerator ShuffleCoroutine()
        {
            for (int i = 0; i < matchableObjects.Count / 2; i++)
            {
                int reverseI = matchableObjects.Count - 1 - i;

                MatchableObjectBehavior first = matchableObjects[i];
                MatchableObjectBehavior second = matchableObjects[reverseI];

                first.transform.DOScale(0, 0.4f).SetEasing(Ease.Type.BackIn);
                second.transform.DOScale(0, 0.4f).SetEasing(Ease.Type.BackIn).OnComplete(() => {
                    first.SwapPositions(second);

                    first.transform.DOScale(1, 0.4f).SetEasing(Ease.Type.BackOut);
                    second.transform.DOScale(1, 0.4f).SetEasing(Ease.Type.BackOut).OnComplete(() => {

                        first.SetActive(first.IsActive);
                        second.SetActive(second.IsActive);
                    });
                });

                yield return new WaitForSeconds(0.07f);
            }

            Tween.DelayedCall(0.3f, () => GameCanvasBehavior.RaycasterEnabled = true);
        }

        public static void RemoveAsTip()
        {
            int slotsAvailable = SlotsController.GetSlotsAvailable();

            if(slotsAvailable > 2)
            {
                // Can remove any three objects

                RemoveRandomThree();
            } else if(slotsAvailable == 2)
            {
                // Can remove only objects with one in slots

                MatchableObject matchable = SlotsController.GetRandomMatchableInSlots();

                int sameInSlots = SlotsController.AmountInSlots(matchable);

                List<MatchableObjectBehavior> sameMatchables = matchableObjects.FindAll((matchableObject) =>
                {
                    return matchableObject.MatchableObject == matchable;
                });

                if(sameInSlots == 2)
                {
                    sameMatchables[0].SetActive(true);
                    RemoveMatchable(sameMatchables[0]);
                } else
                {
                    sameMatchables[0].SetActive(true);
                    sameMatchables[1].SetActive(true);
                    RemoveMatchables(sameMatchables[0], sameMatchables[1]);
                }

                SlotsController.DisableRevert();
            } else
            {
                // Can remove only with two in slots

                MatchableObject matchable = SlotsController.GetMatchableOfTwo();

                if (matchable == null) return;

                MatchableObjectBehavior matchableObject = matchableObjects.Find((MatchableObjectBehavior matchableBehavior) => { return matchableBehavior.MatchableObject == matchable; });

                matchableObject.SetActive(true);
                RemoveMatchable(matchableObject);

                SlotsController.DisableRevert();
            }

            Tween.DelayedCall(0.4f, () => GameCanvasBehavior.RaycasterEnabled = true);
        }

        private static void RemoveRandomThree()
        {
            // finding first occupied layer

            int layerId = -1;
            for (int i = 0; i < Level.AmountOfLayers; i++)
            {
                if (LevelRepresentation.HasLayer(i))
                {
                    layerId = i;
                    break;
                }
            }

            if (layerId == -1) return;

            // Getting random object from the top layer

            MatchableObjectBehavior matchableObject = matchableObjects.FindAll((matchable) => {
                return matchable.LayerId == layerId;
            }).GetRandomItem();

            // Checking how many same objects are in the slots

            int sameInSlots = SlotsController.AmountInSlots(matchableObject.MatchableObject);

            // Finding all eligible objects

            List<MatchableObjectBehavior> sameMatchables = matchableObjects.FindAll((matchable) =>
            {
                return matchable.MatchableObject == matchableObject.MatchableObject && matchableObject != matchable;
            });

            // Moving objects that are in the level to slots

            if(sameMatchables.Count > 1)
            {
                if(sameInSlots == 2)
                {
                    RemoveMatchable(matchableObject);
                } else if(sameInSlots == 1)
                {
                    sameMatchables[0].SetActive(true);
                    RemoveMatchables(matchableObject, sameMatchables[0]);
                } else
                {
                    sameMatchables[0].SetActive(true);
                    sameMatchables[1].SetActive(true);
                    RemoveMatchables(matchableObject, sameMatchables[0], sameMatchables[1]);
                }
                
            } else if(sameMatchables.Count == 1)
            {
                sameMatchables[0].SetActive(true);
                RemoveMatchables(matchableObject, sameMatchables[0]);
            } else
            {
                RemoveMatchable(matchableObject);
            }

            // Making sure revert button cannot be clicked

            SlotsController.DisableRevert();
        }

        public  static void CheckLevelFinish()
        {
            if (matchableObjects.IsNullOrEmpty() && SlotsController.IsEmpty)
            {
                
                CameraBehavior.ExplodeConfetti();

                AudioController.PlaySound(AudioController.Sounds.winSound);

                Tween.DelayedCall(0.5f, LevelCompleteCanvasBehavior.Show);
            }
        }

        public static void DisposeLevel()
        {
            matchableObjects.DOAction((start, end, t) =>
            {
                for (int i = 0; i < matchableObjects.Count; i++)
                {
                    matchableObjects[i].transform.localScale = Vector3.one * (start + (end - start) * t);
                }
            }, 1, 0, 0.4f).SetEasing(Ease.Type.BackIn).OnComplete(() => { 
                matchableObjects.Clear(); 

                foreach(MatchableObject matchable in matchableObjectsPools.Keys)
                {
                    matchableObjectsPools[matchable].ReturnToPoolEverything();
                }
            });

            SlotsController.Dispose();
        }

        public static void DisposeLevelQuickly()
        {
            for (int i = 0; i < matchableObjects.Count; i++)
            {
                matchableObjects[i].transform.localScale = Vector3.zero;
            }

            matchableObjects.Clear();

            foreach (MatchableObject matchable in matchableObjectsPools.Keys)
            {
                matchableObjectsPools[matchable].ReturnToPoolEverything();
            }

            SlotsController.DisposeQuickly();
        }
    }

    public class LevelRepresentation
    {
        public List<LayerRepresentation> layers;

        public void Init(Level level)
        {
            if (layers == null) layers = new List<LayerRepresentation>();

            layers.Clear();

            for(int i = 0; i < level.AmountOfLayers; i++)
            {
                LayerRepresentation layerRepresentation = new LayerRepresentation();

                Layer layer = level.GetLayer(i);

                layerRepresentation.grid = new bool[layer.AmountOfRows, layer.AmountOfRows];

                for(int x = 0; x < layer.AmountOfRows; x++)
                {
                    for (int y = 0; y < layer.AmountOfRows; y++)
                    {
                        layerRepresentation.grid[y, x] = layer[y][x];
                    }
                }

                layers.Add(layerRepresentation);
            }
        }

        public bool HasLayer(int layerId)
        {
            int size = layerId % 2 == 0 ? LevelController.LevelEvenSize : LevelController.LevelOddSize;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (layers[layerId].grid[y, x]) return true;
                }
            }

            return false;
        }

        public bool IsAvailable(MatchableObjectBehavior matchable)
        {
            return IsAvailable(matchable.LayerId, matchable.X, matchable.Y);
        }

        public bool IsAvailable(int layer, int x, int y)
        {
            if (layer == 0) return true;

            bool isEven = layer % 2 == 0;

            for(int i = layer - 1; i >= 0; i--)
            {
                bool isLayerEven = i % 2 == 0;

                if(isEven == isLayerEven)
                {
                    // if there is something directly above object, it is not available

                    if (layers[i].grid[y, x]) return false;
                } else
                {
                    int size = isLayerEven ? LevelController.LevelEvenSize : LevelController.LevelOddSize;

                    int xx = isLayerEven ? x - 1 : x;
                    int yy = isLayerEven ? y - 1 : y;

                    // Checking if there is something partly above the object. If there is - it is not available

                    if (xx != -1 && yy != -1 && layers[i].grid[yy, xx]) return false;
                    xx++;
                    if(xx != size && yy != -1 && layers[i].grid[yy, xx]) return false;
                    yy++;
                    if (xx != 0 && yy != size && layers[i].grid[yy, xx - 1]) return false;
                    if (xx != size && yy != size && layers[i].grid[yy, xx]) return false;
                }
            }

            return true;
        }
    }

    public class LayerRepresentation
    {
        public bool[,] grid;
    }
}