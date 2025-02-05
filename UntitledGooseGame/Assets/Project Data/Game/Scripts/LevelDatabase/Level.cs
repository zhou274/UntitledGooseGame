#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(menuName = "Content/Level Database/Level", fileName = "Level")]
    public class Level : ScriptableObject
    {
        [SerializeField] Layer[] layers;
        public int AmountOfLayers => layers.Length;

        [SerializeField] int firstLayerSize;
        public int FirstLayerSize => firstLayerSize;


        public Layer GetLayer(int i)
        {
            if (i < AmountOfLayers && i >= 0) return layers[i];

            return null;
        }


        public int GetAmountOfFilledCells()
        {
            int counter = 0;

            for (int i = 0; i < AmountOfLayers; i++)
            {
                counter += layers[i].GetAmountOfFilledCells();
            }

            return counter;
        }
    }

    [System.Serializable]
    public class Layer
    {
        [SerializeField] LayerRow[] rows;

        public int AmountOfRows => rows.Length;

        public LayerRow this[int i]
        {
            get => rows[i];
        }

        public LayerRow GetRow(int i)
        {
            if (i < AmountOfRows && i >= 0) return rows[i];

            return null;
        }

        public int GetAmountOfFilledCells()
        {
            int counter = 0;

            for(int i = 0; i < AmountOfRows; i++)
            {
                counter += rows[i].GetAmountOfFilledCells();
            }

            return counter;
        }
    }

    [System.Serializable]
    public class LayerRow
    {
        [SerializeField] bool[] cells;

        public int AmountOfCells => cells.Length;

        public bool this[int i]
        {
            get => cells[i];
        }

        public bool GetCell(int i)
        {
            if (i < AmountOfCells && i >= 0) return cells[i];

            return false;
        }

        public int GetAmountOfFilledCells()
        {
            int counter = 0;

            for (int i = 0; i < AmountOfCells; i++)
            {
                if (cells[i]) counter++;
            }

            return counter;
        }
    }
}