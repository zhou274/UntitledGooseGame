using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using static Watermelon.ProjectCleanUpTool.EditorTable.ColumnData;

namespace Watermelon.ProjectCleanUpTool
{
    public class EditorTable
    {
        private List<Texture> typeTextures;
        private List<DisplayFile> allDisplayFiles;
        private List<DisplayFile> displayFiles;
        private List<ColumnData> columnDatas;

        private const float IMAGE_SIZE  = 16;
        private const float ROW_HEIGHT = 24;
        private const float SPACE = 4;
        private const float BUTTON_SKIN_SPACE = 8;
        private const float CONTENT_PADDING = 2;
        private const char descendingSortChar = '↓';
        private const char ascendingSortChar = '↑';
        
        private bool contentWidthUpToData;
        private bool stylesIsSet;
        private GUIStyle rowStyle;
        private Vector2 scrollVector;
        private float tempPosY;
        private float tempPosX;
        private Rect elementRect;
        private Rect startRect;
        private float endOfLinePosition;
        private int selectedDisplayFileIndex;
        private Category currentCategory;
        private UnityEngine.Object selectedObject;
        private Rect rowRect;
        private Color rowColor1;
        private Color rowColor2;
        private Color selectedRowColor;
        private Color defaultColor;

        //sort
        private string[] sizeEndings = { "b", "kb", "mb", "gb" };
        private int[] extensionsSortIndexes;
        private const string EMPTY_SIZE = "-";
        private string[] el1Split;
        private string[] el2Split;
        private char[] splitArray = { ' ' };
        private int sortColumnIndex;
        private bool descendingSortOrder;
        private bool firstValueMissing;

        public EditorTable(List<DisplayFile> displayFiles, List<Texture> typeTextures)
        {
            this.columnDatas = new List<ColumnData>();
            this.typeTextures = typeTextures;
            this.allDisplayFiles = displayFiles;
            this.displayFiles = new List<DisplayFile>();
            this.displayFiles.AddRange(displayFiles);
            columnDatas.Add(new ColumnData("Type", Columns.Icon, ColumnType.Image));
            columnDatas.Add(new ColumnData("Filename", Columns.FileName, ColumnType.Text));
            columnDatas.Add(new ColumnData("Build size", Columns.BuildSize, ColumnType.Text));
            columnDatas.Add(new ColumnData("Disk size", Columns.DiskSize, ColumnType.Text));
            columnDatas.Add(new ColumnData("Usages", Columns.Usages, ColumnType.Text));
            columnDatas.Add(new ColumnData("Show usages", Columns.ShowUsages, ColumnType.Button));
            columnDatas.Add(new ColumnData("Remove", Columns.Remove, ColumnType.Button));
            contentWidthUpToData = false;
            stylesIsSet = false;
            selectedObject = null;
            selectedDisplayFileIndex = -1;
            currentCategory = Category.NoLimits;
            sortColumnIndex = -1;
            descendingSortOrder = false;

            //handke extensions
            string[] extensions = DatabaseHandler.Instance.GetFileTypeExtensions();
            List<string> extensionsSorted = new List<string>();
            extensionsSorted.AddRange(extensions);
            extensionsSorted.Sort();
            extensionsSortIndexes = new int[extensionsSorted.Count];

            for (int i = 0; i < extensionsSortIndexes.Length; i++)
            {
                extensionsSortIndexes[i] = extensionsSorted.IndexOf(extensions[i]);
            }
        }

        public void SetCategory(Category category)
        {
            if(currentCategory == category)
            {
                return;
            }

            selectedDisplayFileIndex = -1;
            selectedObject = null;
            Selection.activeObject = null;
            currentCategory = category;
            displayFiles.Clear();

            if (category == Category.NoLimits)
            {
                displayFiles.AddRange(allDisplayFiles);
            }
            else if(category == Category.InBuild)
            {
                for (int i = 0; i < allDisplayFiles.Count; i++)
                {
                    if (allDisplayFiles[i].usedInBuild)
                    {
                        displayFiles.Add(allDisplayFiles[i]);
                    }
                }
            }
            else if (category == Category.NotInBuild)
            {
                for (int i = 0; i < allDisplayFiles.Count; i++)
                {
                    if (!allDisplayFiles[i].usedInBuild)
                    {
                        displayFiles.Add(allDisplayFiles[i]);
                    }
                }
            }
            else if (category == Category.Used)
            {
                for (int i = 0; i < allDisplayFiles.Count; i++)
                {
                    if (allDisplayFiles[i].usages != 0)
                    {
                        displayFiles.Add(allDisplayFiles[i]);
                    }
                }
            }
            else if (category == Category.Unused)
            {
                for (int i = 0; i < allDisplayFiles.Count; i++)
                {
                    if (allDisplayFiles[i].usages == 0)
                    {
                        displayFiles.Add(allDisplayFiles[i]);
                    }
                }
            }
        }

        public void DrawTable()
        {
            if (!stylesIsSet)
            {
                UpdateStyles();
            }

            if (!contentWidthUpToData)
            {
                UpdateContentWidth();
            }

            startRect = EditorGUILayout.BeginVertical();
            DrawHeader();
            GUILayout.Space(ROW_HEIGHT);
            EditorGUILayout.EndVertical();

            scrollVector = EditorGUILayout.BeginScrollView(scrollVector);
            EditorGUILayout.BeginVertical();
            startRect = GUILayoutUtility.GetRect(endOfLinePosition - startRect.xMin, displayFiles.Count * ROW_HEIGHT);
            DrawContent();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        

        private void DrawHeader()
        {
            tempPosY = startRect.position.y;
            tempPosX = startRect.position.x + SPACE;

            for (int i = 0; i < columnDatas.Count; i++)
            {
                if (!columnDatas[i].isDisplayed)
                {
                    continue;
                }

                elementRect = new Rect(tempPosX,tempPosY, columnDatas[i].maxContentWidth , ROW_HEIGHT);
                columnDatas[i].positionX = tempPosX;

                if (columnDatas[i].columnType == ColumnType.Button)
                {
                    GUI.Label(elementRect, columnDatas[i].name);
                }
                else
                {
                    if (GUI.Button(elementRect, GetModifiedColumnName(i)))
                    {
                        ProcessSortClick(i);
                    }
                }

                tempPosX += columnDatas[i].maxContentWidth + SPACE;

            }

            endOfLinePosition = tempPosX;
        }

        private string GetModifiedColumnName(int columnIndex)
        {
            if (sortColumnIndex != columnIndex)
            {
                return columnDatas[columnIndex].name;
            }
            else
            {
                if (descendingSortOrder)
                {
                    return columnDatas[columnIndex].name + descendingSortChar;
                }
                else
                {
                    return columnDatas[columnIndex].name + ascendingSortChar;
                }
            }
        }


        private void ProcessSortClick(int columnIndex)
        {
            if(sortColumnIndex == columnIndex)
            {
                descendingSortOrder = !descendingSortOrder;
                displayFiles.Reverse();
                return;
            }
            else
            {
                sortColumnIndex = columnIndex;
                descendingSortOrder = false;
            }

            if(columnDatas[columnIndex].column == Columns.Icon)
            {
                displayFiles.Sort((x, y) => extensionsSortIndexes[x.textureIndex].CompareTo(extensionsSortIndexes[y.textureIndex]));
            }
            else if(columnDatas[columnIndex].column == Columns.FileName)
            {
                displayFiles.Sort((x, y) => x.filename.CompareTo(y.filename));
            }
            else if(columnDatas[columnIndex].column == Columns.BuildSize)
            {
                displayFiles.Sort((x, y) => CompareSize(x.buildSize, y.buildSize));
            }
            else if (columnDatas[columnIndex].column == Columns.DiskSize)
            {
                displayFiles.Sort((x, y) => CompareSize(x.diskSize, y.diskSize));
            }
            else if (columnDatas[columnIndex].column == Columns.Usages)
            {
                displayFiles.Sort((x, y) => x.usages.CompareTo(y.usages));
            }
        }

        private int CompareSize(string el1, string el2)
        {
            if (!el1.Contains(EMPTY_SIZE))
            {
                el1Split = el1.Split(splitArray, 2);
                firstValueMissing = false;
            }
            else
            {
                firstValueMissing = true;
            }

            if (!el2.Contains(EMPTY_SIZE))
            {
                if (firstValueMissing)
                {
                    return -1;
                }

                el2Split = el2.Split(splitArray, 2);
            }
            else
            {
                if (firstValueMissing)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }

            if (el1Split[1].Equals(el2Split[1])) //need to parce and compare float
            {
                return float.Parse(el1Split[0]).CompareTo(float.Parse(el2Split[0]));
            }
            else
            {
                for (int i = 0; i < sizeEndings.Length; i++)
                {
                    if (el1Split[1].Equals(sizeEndings[i]))
                    {
                        return -1;
                    }
                    else if (el2Split[1].Equals(sizeEndings[i]))
                    {
                        return 1;
                    }
                }

                Debug.LogError("failed to compare:" + el1 + " and " + el2);
                return 0;
            }
        }

        private void DrawContent()
        {
            tempPosY = startRect.position.y;
            HandleProjectWindowSelection();

            for (int i = 0; i < displayFiles.Count; i++)
            {
                for (int j = 0; j < columnDatas.Count; j++)
                {
                    if (!columnDatas[j].isDisplayed)
                    {
                        continue;
                    }

                    rowRect = new Rect(startRect.position.x, tempPosY, endOfLinePosition - startRect.position.x, ROW_HEIGHT);

                    if (i == selectedDisplayFileIndex)
                    {
                        DrawColorRect(rowRect, selectedRowColor);
                    }
                    else
                    {
                        if (i % 2 == 0)
                        {
                            DrawColorRect(rowRect, rowColor1);
                        }
                        else
                        {
                            DrawColorRect(rowRect, rowColor2);
                        }
                    }


                    if (columnDatas[j].columnType == ColumnType.Image)
                    {
                        elementRect = new Rect(columnDatas[j].positionX, tempPosY + CONTENT_PADDING, IMAGE_SIZE, IMAGE_SIZE);
                        GUI.DrawTexture(elementRect, typeTextures[displayFiles[i].textureIndex]);
                    }
                    else if (columnDatas[j].columnType == ColumnType.Button)
                    {
                        elementRect = new Rect(columnDatas[j].positionX, tempPosY + CONTENT_PADDING, columnDatas[j].maxContentWidth, ROW_HEIGHT - CONTENT_PADDING * 2);
                        if (GUI.Button(elementRect, columnDatas[j].name))
                        {
                            if(columnDatas[j].column == Columns.ShowUsages)
                            {
                                EditorUtility.DisplayDialog(displayFiles[i].filename + " " + displayFiles[i].usages + " usages:", DatabaseHandler.Instance.GetUsagesPaths(displayFiles[i].originalIndex), "Ok");
                            }
                            else if (columnDatas[j].column == Columns.Remove)
                            {
                                if (EditorUtility.DisplayDialog("Warning", "Are you sure you want to remove file \"" + displayFiles[i].filename + "\" ?", "Ok", "Cancel"))
                                {
                                    DatabaseHandler.Instance.ProcessDeleteFile(displayFiles[i].originalIndex);
                                    displayFiles.RemoveAt(i);
                                    selectedDisplayFileIndex = -1;
                                    selectedObject = null;
                                    Selection.activeObject = null;

                                    for (int index = 0; index < displayFiles.Count; index++)
                                    {
                                        displayFiles[i].usages = DatabaseHandler.Instance.UpdateUsages(displayFiles[i].originalIndex);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        elementRect = new Rect(columnDatas[j].positionX, tempPosY, columnDatas[j].maxContentWidth, ROW_HEIGHT);
                        GUI.Label(elementRect, displayFiles[i].GetFieldAsString(columnDatas[j].column));
                    }
                }

                //handle element selection
                if (GUI.Button(rowRect, GUIContent.none, GUIStyle.none))
                {
                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(displayFiles[i].guid));
                    selectedDisplayFileIndex = i;
                    selectedObject = Selection.activeObject;
                }

                tempPosY += ROW_HEIGHT;
            }
        }

        private void HandleProjectWindowSelection()
        {
            if (Selection.activeObject == null)
            {
                selectedObject = null;
                selectedDisplayFileIndex = -1;
            }
            else if (Selection.activeObject != selectedObject)
            {
                selectedObject = Selection.activeObject;
                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(selectedObject));
                selectedDisplayFileIndex = -1;

                for (int i = 0; i < displayFiles.Count; i++)
                {
                    if (displayFiles[i].guid.Equals(guid))
                    {
                        selectedDisplayFileIndex = i;
                        scrollVector = new Vector2(scrollVector.x, selectedDisplayFileIndex * ROW_HEIGHT - startRect.height);
                        break;
                    }
                }
            }
        }

        private void DrawColorRect(Rect rect,Color color)
        {
            defaultColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
            GUI.color = defaultColor;
        }

        private void UpdateContentWidth()
        {
            for (int i = 0; i < columnDatas.Count; i++)
            {
                columnDatas[i].maxCharacterLength = columnDatas[i].name.Length;
                columnDatas[i].maxContentWidth = GetTextWidth(columnDatas[i].name + descendingSortChar) + BUTTON_SKIN_SPACE;
            }

            if(displayFiles == null)
            {
                return;
            }

            string value;
            float width;

            for (int i = 0; i < displayFiles.Count; i++)
            {
                for (int j = 0; j < columnDatas.Count; j++)
                {
                    if(columnDatas[j].columnType != ColumnType.Text)
                    {
                        continue;
                    }

                    value = displayFiles[i].GetFieldAsString(columnDatas[j].column);

                    if(value.Length >= columnDatas[j].maxCharacterLength)
                    {
                        width = GetTextWidth(value);

                        if(width > columnDatas[j].maxContentWidth)
                        {
                            columnDatas[j].maxContentWidth = width;
                        }
                    }
                }
            }
        }

        private void UpdateStyles()
        {
            stylesIsSet = true;
            rowStyle = new GUIStyle();
            rowStyle.stretchHeight = false;
            rowStyle.stretchWidth = true;

            rowColor1 = new Color(0.3f, 0.3f, 0.3f,0.1f);
            rowColor2 = new Color(0.2f, 0.2f, 0.2f, 0.1f);
            selectedRowColor = new Color(0.9f, 0.9f, 0.9f, 0.1f);

        }

        public float GetTextWidth(string text)
        {
            return GUI.skin.label.CalcSize(new GUIContent(text)).x;
        }

        public float GetTextWidth(GUIContent content)
        {
            return GUI.skin.label.CalcSize(content).x;
        }

        public class DisplayFile
        {
            public int originalIndex;
            public string filename;
            public int textureIndex;
            public string diskSize;
            public string buildSize;
            public bool usedInBuild;
            public string usedInBuildString;
            public int usages;
            public string guid;

            public string GetFieldAsString(Columns field)
            {
                switch (field)
                {
                    case Columns.FileName:
                        return filename;
                    case Columns.DiskSize:
                        return diskSize;
                    case Columns.BuildSize:
                        return buildSize;
                    case Columns.Usages:
                        return usages.ToString();
                    default:
                        return string.Empty;
                }
            }

            
        }

        public class ColumnData
        {
            public string name;
            public Columns column;
            public ColumnType columnType;
            public float maxContentWidth;
            public int maxCharacterLength;
            public bool isDisplayed;
            public float positionX;

            public ColumnData(string name, Columns column, ColumnType columnType)
            {
                this.name = name;
                this.column = column;
                this.columnType = columnType;
                this.maxCharacterLength = 0;
                this.maxContentWidth = 0;
                this.isDisplayed = true;
            }

            public enum Columns
            {
                Icon,
                FileName,
                DiskSize,
                BuildSize,
                Usages,
                ShowUsages,
                Remove
            }

            public enum ColumnType
            {
                Image,
                Text,
                Button
            }
        }

        public enum Category
        {
            NoLimits,
            Unused,
            Used,
            InBuild,
            NotInBuild
        }
    }
}