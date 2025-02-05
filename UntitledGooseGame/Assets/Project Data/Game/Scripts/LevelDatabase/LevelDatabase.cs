#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(menuName = "Content/Level Database/Level Database", fileName = "Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        [SerializeField] Level[] levels;
        [SerializeField] MatchableObject[] matchableObjects;

        //editor stuff
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color unselectedColor;
        [SerializeField] private Color tintColor;
        [SerializeField] private Color backgroundColor;
        [SerializeField] private Color gridColor;

        public int AmountOfLevels => levels.Length;
        public int AmountOfMatchableObjects => matchableObjects.Length;

        public Level GetLevel(int i)
        {
            if (i < AmountOfLevels && i >= 0) return levels[i];

            return null;
        }

        public MatchableObject GetmatchableObject(int i)
        {
            if (i < AmountOfMatchableObjects && i >= 0) return matchableObjects[i];

            return null;
        }

        public List<MatchableObject> AvailableForLevel(Level level)
        {
            int levelIndex = -1;

            for(int i = 0; i < AmountOfLevels; i++)
            {
                if(levels[i] == level)
                {
                    levelIndex = i;
                    break;
                }
            }

            return AvailableForLevel(levelIndex);
        }

        public List<MatchableObject> AvailableForLevel(int levelId)
        {
            List<MatchableObject> result = new List<MatchableObject>();

            for(int i = 0; i < AmountOfMatchableObjects; i++)
            {
                if(matchableObjects[i].AvailableFromLevel <= levelId)
                {
                    result.Add(matchableObjects[i]);
                }
            }

            return result;
        }
    }

    [System.Serializable]
    public class MatchableObject
    {
        [SerializeField] GameObject prefab;
        [SerializeField] int availableFromLevel;

        public GameObject Prefab => prefab;
        public int AvailableFromLevel => availableFromLevel;
    }
}
