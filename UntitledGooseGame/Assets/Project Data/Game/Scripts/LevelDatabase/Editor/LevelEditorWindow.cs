#pragma warning disable 649

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using UnityEditorInternal;
using System.IO;
using System.Text.RegularExpressions;

namespace Watermelon
{
    public class LevelEditorWindow : LevelEditorBase
    {

        //used variables
        private const string RETURN_LABEL = "←";
        private LevelRepresentation selectedLevelRepresentation;
        private LevelsHandler levelsHandler;
        private bool editingSelectedLevel;
        private TabHandler levelTabHandler;
        private TabHandler tabHandler;
        private string levelLabel;

        //level drawing
        private Rect drawRect;
        private float xSize;
        private float ySize;
        private float elementSize;
        private Event currentEvent;
        private Vector2 elementUnderMouseIndex;
        private Vector2Int elementPosition;
        private float buttonRectX;
        private float buttonRectY;
        private const int BOTTOM_PADDING = 8;
        private int selectedLayerIndex;
        private string[] layersOptions;
        private Color defaultColor;
        private float maxFieldSize;
        private int maxLayerSize;
        private float halfElementSize;
        private TintMap tintMap;
        private int layerSize;
        private float elementOffset;
        private float defaultLabelWidth;

        //Database 
        private const string LEVELS_PROPERTY_NAME = "levels";
        private const string MATCHABLE_OBJECTS_PROPERTY_NAME = "matchableObjects";
        private const string SELECTED_COLOR_PROPERTY = "selectedColor";
        private const string UNSELECTED_COLOR_PROPERTY = "unselectedColor";
        private const string TINT_COLOR_PROPERTY = "tintColor";
        private const string BACKGROUND_COLOR_PROPERTY = "backgroundColor";
        private const string GRID_COLOR_PROPERTY = "gridColor";
        private SerializedProperty levelsSerializedProperty;
        private SerializedProperty matchableObjectsProperty;
        private SerializedProperty selectedColorSerializedProperty;
        private SerializedProperty unselectedColorSerializedProperty;
        private SerializedProperty tintColorSerializedProperty;
        private SerializedProperty backgroundColorSerializedProperty;
        private SerializedProperty gridColorSerializedProperty;

        

        

        //Draw levels Tab
        private const string LEVELS_TAB_NAME = "Levels";
        private const float LIST_AREA_WIDTH = 320f;
        private const string EDIT_LEVEL = "Edit level";
        private const string SELECT_LEVEL = "Set this level as current level";

        //edit layers tab
        private const string EDIT_LAYERS_TAB_NAME = "Edit layers";
        private const string FILE = "file:";
        private const string MOUSE_BUTTONS_INSTRUCTION = "Use left click to select cell or left click to unselect. You can also hold one of mouse buttons and drag.";
        private const string MOUSE_WHEEL_INSTRUCTION = "Use mouse whell to switch layer.";

        private const int INFO_HEIGH = 122; //found out using Debug.Log(infoRect) on worst case scenario
        private const string LEVEL_PASSED_VALIDATION = "Level passed validation.";
        private const string LAYERS_LABEL = "Layers";
        private const string ADD_LAYER_LABEL = "Add layer";
        private const string CURRENT_LAYER_LABEL = "Current layer:";
        private const string REMOVE_LAST_LAYER = "Remove last layer";
        private const string WARNING_LABEL = "Warning";
        private const string OK_LABEL = "Ok";
        private const string CANCEL_LABEL = "Cancel";
        private const string LAYER_REMOVE_WARNING = "Are you sure you want to remove last layer ?";

        //edit variables tab
        private const string EDIT_VARIABLES_TAB_NAME = "Edit variables";
        private Rect infoRect;

        //Matchable objects tab
        private const string MATCHABLE_OBJECTS_TAB_NAME = "Matchable objects";
        private const string PREFAB_PROPERTY_NAME = "prefab";
        private const string AVAILABLE_FROM_LEVEL_PROPERTY_NAME = "availableFromLevel";
        private const float PROPERTIES_WIDTH_MARGIN = 4;

        private ReorderableList matchableObjectsReordableList;
        private Rect firstHalfRect;
        private Rect secondHalfRect;

        protected override WindowConfiguration SetUpWindowConfiguration(WindowConfiguration.Builder builder)
        {
            return builder.SetWindowMinSize(new Vector2(700, 500)).Build();
        }

        protected override Type GetLevelsDatabaseType()
        {
            return typeof(LevelDatabase);
        }

        public override Type GetLevelType()
        {
            return typeof(Level);
        }

        protected override void ReadLevelDatabaseFields()
        {
            levelsSerializedProperty = levelsDatabaseSerializedObject.FindProperty(LEVELS_PROPERTY_NAME);
            matchableObjectsProperty = levelsDatabaseSerializedObject.FindProperty(MATCHABLE_OBJECTS_PROPERTY_NAME);
            
            selectedColorSerializedProperty = levelsDatabaseSerializedObject.FindProperty(SELECTED_COLOR_PROPERTY);
            unselectedColorSerializedProperty = levelsDatabaseSerializedObject.FindProperty(UNSELECTED_COLOR_PROPERTY);
            tintColorSerializedProperty = levelsDatabaseSerializedObject.FindProperty(TINT_COLOR_PROPERTY);
            backgroundColorSerializedProperty = levelsDatabaseSerializedObject.FindProperty(BACKGROUND_COLOR_PROPERTY);
            gridColorSerializedProperty = levelsDatabaseSerializedObject.FindProperty(GRID_COLOR_PROPERTY);
            
        }

        protected override void InitialiseVariables()
        {
            levelsHandler = new LevelsHandler(levelsDatabaseSerializedObject, levelsSerializedProperty);
            PrefsSettings.InitEditor();
            tabHandler = new TabHandler();
            tabHandler.AddTab(new TabHandler.Tab(LEVELS_TAB_NAME, DrawLevelsTab));
            tabHandler.AddTab(new TabHandler.Tab(MATCHABLE_OBJECTS_TAB_NAME, DrawMatchableObjectsTab,OnBeforeOpenedMatchableObjectsTab));
            levelTabHandler = new TabHandler();
            levelTabHandler.AddTab(new TabHandler.Tab(EDIT_LAYERS_TAB_NAME, DrawEditLayersTab));
            levelTabHandler.AddTab(new TabHandler.Tab(EDIT_VARIABLES_TAB_NAME, DrawEditVariablesTab));
            defaultColor = GUI.color;

        }


        protected override void Styles()
        {
            if (tabHandler != null)
            {
                tabHandler.SetDefaultToolbarStyle();
                levelTabHandler.SetDefaultToolbarStyle();
            }
        }

        public override void OpenLevel(UnityEngine.Object levelObject, int index)
        {
            selectedLevelRepresentation = new LevelRepresentation(levelObject);
        }

        public override string GetLevelLabel(UnityEngine.Object levelObject, int index)
        {
            return new LevelRepresentation(levelObject).GetLevelLabel(index, stringBuilder);
        }

        public override void ClearLevel(UnityEngine.Object levelObject)
        {
            new LevelRepresentation(levelObject).Clear();
        }
        public override void LogErrorsForGlobalValidation(UnityEngine.Object levelObject, int index)
        {
            LevelRepresentation level = new LevelRepresentation(levelObject);
            level.ValidateLevel();

            if (!level.IsLevelCorrect)
            {
                Debug.Log("Logging validation errors for level #" + (index + 1) + " :");

                foreach (string error in level.errorLabels)
                {
                    Debug.LogWarning(error);
                }
            }
            else
            {
                Debug.Log("Level # " + +(index + 1) + " passed validation.");
            }
            
        }

        protected override void DrawContent()
        {
            if (editingSelectedLevel)
            {
                DisplayEditingLevelHead();
                levelTabHandler.DisplayTab();
            }
            else
            {
                tabHandler.DisplayTab();
            }
        }

        private void DisplayEditingLevelHead()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(RETURN_LABEL, EditorStylesExtended.button_01, GUILayout.Width(40)))
            {
                editingSelectedLevel = false;
                levelTabHandler.SetTabIndex(0);
            }

            GUILayout.Space(20);

            EditorGUILayout.LabelField(levelLabel, EditorStylesExtended.label_medium);

            EditorGUILayout.EndHorizontal();
        }

        #region Levels Tab
        private void DrawLevelsTab()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();

            if (levelsHandler.SelectedLevelIndex == -1)
            {
                DisplaySidebar();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                DisplaySidebar();
                EditorGUILayout.Space();
                DisplaySelectedLevel();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }
        private void DisplaySidebar()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(LIST_AREA_WIDTH));
            levelsHandler.DisplayReordableList();
            levelsHandler.DrawRenameLevelsButton();
            levelsHandler.DrawGlobalValidationButton();
            EditorGUILayout.EndVertical();
        }
        private void DisplaySelectedLevel()
        {
            EditorGUILayout.BeginVertical();

            if (levelsHandler.SelectedLevelIndex == -1)
            {
                EditorGUILayout.Space();
                return;
            }

            //handle level file field
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(levelsHandler.SelectedLevelProperty, new GUIContent(FILE));


            if (EditorGUI.EndChangeCheck())
            {
                levelsHandler.ReopenLevel();
            }

            if (selectedLevelRepresentation.NullLevel)
            {
                return;
            }

            EditorGUILayout.Space();

            if (GUILayout.Button(SELECT_LEVEL, EditorStylesExtended.button_01, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
            {
                SetAsCurrentLevel();
            }

            if (GUILayout.Button(EDIT_LEVEL, EditorStylesExtended.button_01, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
            {
                OpenLevel(levelsHandler.SelectedLevelProperty.objectReferenceValue, levelsHandler.SelectedLevelIndex);
                levelLabel = selectedLevelRepresentation.GetLevelLabel(levelsHandler.SelectedLevelIndex, stringBuilder);
                editingSelectedLevel = true;
                UpdateLayersOptions();
            }

            EditorGUILayout.EndVertical();
        }

        private void UpdateLayersOptions()
        {
            selectedLayerIndex = -1;
            layersOptions = new string[selectedLevelRepresentation.layersProperty.arraySize];

            for (int i = 0; i < layersOptions.Length; i++)
            {
                layersOptions[i] = selectedLevelRepresentation.GetLayerName(i);
            }
        }

        private void SetAsCurrentLevel()
        {
            PrefsSettings.SetInt(PrefsSettings.Key.ActualLevelID, levelsHandler.SelectedLevelIndex);
        }

        #endregion


        #region matchable objects tab

        private void OnBeforeOpenedMatchableObjectsTab()
        {
            matchableObjectsReordableList = new ReorderableList(levelsDatabaseSerializedObject, matchableObjectsProperty);
            matchableObjectsReordableList.drawHeaderCallback += MatchableObjectsDrawHeader;
            matchableObjectsReordableList.drawElementCallback += MatchableObjectsDrawElement;
        }

        private void MatchableObjectsDrawHeader(Rect rect)
        {
            GUI.Label(rect, MATCHABLE_OBJECTS_TAB_NAME);
        }

        private void MatchableObjectsDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            firstHalfRect = new Rect(rect.x, rect.y, (rect.width / 2f) - PROPERTIES_WIDTH_MARGIN, rect.height);
            secondHalfRect = new Rect(rect.x + firstHalfRect.width + (PROPERTIES_WIDTH_MARGIN * 2f), rect.y, firstHalfRect.width, rect.height);
            EditorGUI.PropertyField(firstHalfRect, matchableObjectsProperty.GetArrayElementAtIndex(index).FindPropertyRelative(PREFAB_PROPERTY_NAME));
            EditorGUI.PropertyField(secondHalfRect, matchableObjectsProperty.GetArrayElementAtIndex(index).FindPropertyRelative(AVAILABLE_FROM_LEVEL_PROPERTY_NAME));
        }

        private void DrawMatchableObjectsTab()
        {
            EditorGUILayout.BeginVertical();
            matchableObjectsReordableList.DoLayoutList();
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Edit Layers Tab
        private void DrawEditLayersTab()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField(LAYERS_LABEL, EditorStylesExtended.label_medium_bold);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(ADD_LAYER_LABEL, EditorStylesExtended.button_01))
            {
                selectedLevelRepresentation.AddLayer();
                UpdateLayersOptions();
                selectedLayerIndex = layersOptions.Length - 1;
            }

            EditorGUI.BeginDisabledGroup(selectedLevelRepresentation.layersProperty.arraySize == 0);

            if (GUILayout.Button(REMOVE_LAST_LAYER, EditorStylesExtended.button_01))
            {
                if (EditorUtility.DisplayDialog(WARNING_LABEL, LAYER_REMOVE_WARNING, OK_LABEL, CANCEL_LABEL))
                {
                    selectedLevelRepresentation.layersProperty.arraySize--;
                    UpdateLayersOptions();
                }
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (IsPropertyChanged(selectedLevelRepresentation.firstLayerSizeProperty))
            {
                selectedLevelRepresentation.HandleSizePropertyChange();
                UpdateLayersOptions();
            }
            //layer selection 
            selectedLayerIndex = EditorGUILayout.Popup(CURRENT_LAYER_LABEL, selectedLayerIndex, layersOptions);
            HandleScrollEvent();

            

            if(selectedLayerIndex != -1)
            {
                DrawLayer();
            }
            else
            {
                GUILayout.FlexibleSpace();
            }

            selectedLevelRepresentation.ValidateLevel();
            selectedLevelRepresentation.ApplyChanges();

            DrawTipsAndWarnings();

            EditorGUILayout.EndVertical();
        }

        private void HandleScrollEvent()
        {
            currentEvent = Event.current;

            if (currentEvent.type == EventType.ScrollWheel)
            {

                if (currentEvent.delta.y != 0)
                {
                    if (currentEvent.delta.y < 0) // scroll up
                    {
                        if (selectedLayerIndex > 0)
                        {
                            selectedLayerIndex--;
                        }
                    }
                    else
                    {
                        if (selectedLayerIndex < layersOptions.Length - 1)
                        {
                            selectedLayerIndex++;
                        }
                    }

                    currentEvent.Use();
                }
            }
        }

        private void DrawLayer()
        {
            drawRect = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            maxFieldSize =  selectedLevelRepresentation.firstLayerSizeProperty.intValue + 1f;
            maxLayerSize = selectedLevelRepresentation.firstLayerSizeProperty.intValue + 1;
            xSize = Mathf.Floor(drawRect.width / maxFieldSize);
            ySize = Mathf.Floor(drawRect.height / maxFieldSize);
            elementSize = Mathf.Min(xSize, ySize);
            halfElementSize = elementSize / 2f;
            layerSize = selectedLevelRepresentation.GetLayerSize(selectedLayerIndex);

            if (layerSize == selectedLevelRepresentation.firstLayerSizeProperty.intValue + 1)
            {
                elementOffset = 0;
            }
            else
            {
                elementOffset = halfElementSize;
            }

            CheckTintMap(maxLayerSize);

            currentEvent = Event.current;

            //Handle drag
            if (currentEvent.type == EventType.MouseDrag)
            {
                elementUnderMouseIndex = (currentEvent.mousePosition - drawRect.position - (Vector2.one * elementOffset)) / (elementSize);

                elementPosition = new Vector2Int(Mathf.FloorToInt(elementUnderMouseIndex.x), Mathf.FloorToInt(elementUnderMouseIndex.y));

                if ((elementPosition.x >= 0) && (elementPosition.x < layerSize) && (elementPosition.y >= 0) && (elementPosition.y < layerSize))
                {
                    selectedLevelRepresentation.SetCellValue(selectedLayerIndex, elementPosition.x, layerSize - elementPosition.y - 1, currentEvent.button == 0);
                    currentEvent.Use();
                }
            }

            Rect buttonRect;

            //draw  background
            GUI.color = backgroundColorSerializedProperty.colorValue;
            GUI.DrawTexture(new Rect(drawRect.position, elementSize * maxLayerSize * Vector2.one), Texture2D.whiteTexture);

            //draw current layer
            for (int rowIndex = layerSize - 1; rowIndex >= 0; rowIndex--)
            {
                for (int columnIndex = 0; columnIndex < layerSize; columnIndex++)
                {
                    buttonRectX = drawRect.position.x + columnIndex * elementSize + elementOffset;
                    buttonRectY = drawRect.position.y + (layerSize - rowIndex - 1) * elementSize + elementOffset;
                    buttonRect = new Rect(buttonRectX, buttonRectY, elementSize, elementSize);

                    if (selectedLevelRepresentation.GetCellValue(selectedLayerIndex, columnIndex, rowIndex))
                    {
                        GUI.color = selectedColorSerializedProperty.colorValue;
                        GUI.DrawTexture(buttonRect, Texture2D.whiteTexture);

                        if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
                        {
                            if (currentEvent.button == 1)
                            {
                                selectedLevelRepresentation.SetCellValue(selectedLayerIndex, columnIndex, rowIndex, false);
                            }
                        }
                    }
                    else
                    {
                        GUI.color = unselectedColorSerializedProperty.colorValue;
                        GUI.DrawTexture(buttonRect, Texture2D.whiteTexture);

                        if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
                        {
                            if (currentEvent.button == 0)
                            {
                                selectedLevelRepresentation.SetCellValue(selectedLayerIndex, columnIndex, rowIndex, true);
                            }
                        }
                    }
                }
            }

            //draw tint
            if (selectedLayerIndex >= 1)
            {
                GUI.color = tintColorSerializedProperty.colorValue;

                foreach (Vector2Int el in tintMap.mapList)
                {
                    GUI.DrawTexture(new Rect(drawRect.position + Vector2.one * halfElementSize * el,Vector2.one * halfElementSize), Texture2D.whiteTexture);
                }
            }

            // draw grid

            GUI.color = gridColorSerializedProperty.colorValue;

            for (int i = 0; i <= layerSize; i++)
            {
                GUI.DrawTexture(new Rect(drawRect.position.x + elementOffset + elementSize * i, drawRect.position.y + elementOffset, 1, elementSize * layerSize), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(drawRect.position.x + elementOffset, drawRect.position.y + elementOffset + elementSize * i, elementSize * layerSize, 1), Texture2D.whiteTexture);
            }

            GUI.color = defaultColor;
            EditorGUILayout.EndVertical();

            GUILayout.Space(BOTTOM_PADDING);
        }

        private void CheckTintMap(int maxLayerSize)
        {
            if (selectedLayerIndex < 1)
            {
                tintMap = null;
                return;
            }

            if ((tintMap == null) || (tintMap.selectedLayer != selectedLayerIndex))
            {
                tintMap = new TintMap(selectedLayerIndex, maxLayerSize);
                int layerSize;

                for (int layerIndex = 0; layerIndex < selectedLayerIndex; layerIndex++)
                {
                    layerSize = selectedLevelRepresentation.GetLayerSize(layerIndex);

                    for (int x = 0; x < layerSize; x++)
                    {
                        for (int y = 0; y < layerSize; y++)
                        {
                            if (selectedLevelRepresentation.GetCellValue(layerIndex, x, y))
                            {
                                if (layerSize == maxLayerSize)
                                {
                                    tintMap.MarkPositionInOuterLayer(x, y);
                                }
                                else
                                {
                                    tintMap.MarkPositionInInnerLayer(x, y);
                                }
                            }
                        }
                    }
                }

                tintMap.FillMapList();
            }
        }

        private void DrawTipsAndWarnings()
        {
            infoRect = EditorGUILayout.BeginVertical(GUILayout.MinHeight(INFO_HEIGH));
            EditorGUILayout.HelpBox(MOUSE_BUTTONS_INSTRUCTION, MessageType.Info);
            EditorGUILayout.HelpBox(MOUSE_WHEEL_INSTRUCTION, MessageType.Info);

            if (selectedLevelRepresentation.IsLevelCorrect)
            {
                EditorGUILayout.HelpBox(LEVEL_PASSED_VALIDATION, MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(selectedLevelRepresentation.errorLabels[0], MessageType.Error);
            }

            EditorGUILayout.EndVertical();

            //Debug.Log(infoRect.height);
        }


        #endregion

        private void DrawEditVariablesTab()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Colors for \"Edit Layers\" Tab", EditorStylesExtended.label_medium_bold);
            EditorGUILayout.PropertyField(selectedColorSerializedProperty);
            EditorGUILayout.PropertyField(unselectedColorSerializedProperty);
            EditorGUILayout.PropertyField(backgroundColorSerializedProperty);
            EditorGUILayout.PropertyField(tintColorSerializedProperty);
            EditorGUILayout.PropertyField(gridColorSerializedProperty);
            EditorGUILayout.EndVertical();
        }

        private class TintMap
        {
            public int selectedLayer;
            public int maxLayerSize;
            public bool[,] map;
            public List<Vector2Int> mapList;

            public TintMap(int selectedLayer, int maxLayerSize)
            {
                this.selectedLayer = selectedLayer;
                this.maxLayerSize = maxLayerSize;
                this.map = new bool[maxLayerSize * 2, maxLayerSize * 2];
                mapList = new List<Vector2Int>();
            }

            public void MarkPositionInOuterLayer(int x, int y) // if firstLayerSize = 5 this function is for every layer with size 6
            {
                int topLeftX = x * 2;
                int topLeftY = (maxLayerSize - y - 1) * 2;

                try
                {
                    map[topLeftX, topLeftY] = true;
                    map[topLeftX + 1, topLeftY] = true;
                    map[topLeftX, topLeftY + 1] = true;
                    map[topLeftX + 1, topLeftY + 1] = true;
                }
                catch (IndexOutOfRangeException e)
                {
                    Debug.LogError("Incorrect layers size.");
                    Debug.LogError(e);
                }
            }

            public void MarkPositionInInnerLayer(int x, int y) // if firstLayerSize = 5 this function is for every layer with size 5
            {
                int topLeftX = x * 2 + 1;
                int topLeftY = (maxLayerSize - y - 2) * 2 + 1;

                try
                {
                    map[topLeftX, topLeftY] = true;
                    map[topLeftX + 1, topLeftY] = true;
                    map[topLeftX, topLeftY + 1] = true;
                    map[topLeftX + 1, topLeftY + 1] = true;
                }
                catch (IndexOutOfRangeException e)
                {
                    Debug.LogError("Incorrect layers size");
                    Debug.LogError(e);
                }
            }

            public void FillMapList()
            {
                for (int i = 0; i < maxLayerSize * 2; i++)
                {
                    for (int j = 0; j < maxLayerSize * 2; j++)
                    {
                        if (map[i, j])
                        {
                            mapList.Add(new Vector2Int(i, j));
                        }
                    }
                }
            }
        }

        protected class LevelRepresentation : LevelRepresentationBase
        {
            private const string LAYERS_PROPERTY_NAME = "layers";
            private const string ROWS_PROPERTY_NAME = "rows";
            private const string CELLS_PROPERTY_NAME = "cells";
            private const string FIRST_LAYER_SIZE_PROPERTY_NAME = "firstLayerSize";

            public SerializedProperty layersProperty;
            public SerializedProperty firstLayerSizeProperty;

            protected override bool LEVEL_CHECK_ENABLED => true;

            public LevelRepresentation(UnityEngine.Object levelObject) : base(levelObject)
            {
            }

            protected override void ReadFields()
            {
                layersProperty = serializedLevelObject.FindProperty(LAYERS_PROPERTY_NAME);
                firstLayerSizeProperty = serializedLevelObject.FindProperty(FIRST_LAYER_SIZE_PROPERTY_NAME);
            }

            public override void Clear()
            {
                layersProperty.arraySize = 0;
                firstLayerSizeProperty.intValue = 0;
                ApplyChanges();
            }

            public bool GetCellValue(int layer,int xIndex, int yIndex)
            {
                return layersProperty.GetArrayElementAtIndex(layer).FindPropertyRelative(ROWS_PROPERTY_NAME).GetArrayElementAtIndex(yIndex).FindPropertyRelative(CELLS_PROPERTY_NAME).GetArrayElementAtIndex(xIndex).boolValue;
            }

            public void SetCellValue(int layer, int xIndex, int yIndex,bool newValue)
            {
                layersProperty.GetArrayElementAtIndex(layer).FindPropertyRelative(ROWS_PROPERTY_NAME).GetArrayElementAtIndex(yIndex).FindPropertyRelative(CELLS_PROPERTY_NAME).GetArrayElementAtIndex(xIndex).boolValue = newValue;
            }

            public void HandleSizePropertyChange()
            {
                if (firstLayerSizeProperty.intValue < 0)
                {
                    firstLayerSizeProperty.intValue = 0;
                }

                int correctSize;

                for (int layerIndex = 0; layerIndex < layersProperty.arraySize; layerIndex++)
                {
                    correctSize = SetCorrectLevelSize(layerIndex);
                }
            }

            private int SetCorrectLevelSize(int layerIndex)
            {
                int correctSize = firstLayerSizeProperty.intValue + (layerIndex % 2);
                layersProperty.GetArrayElementAtIndex(layerIndex).FindPropertyRelative(ROWS_PROPERTY_NAME).arraySize = correctSize;

                for (int i = 0; i < correctSize; i++)
                {
                    layersProperty.GetArrayElementAtIndex(layerIndex).FindPropertyRelative(ROWS_PROPERTY_NAME).GetArrayElementAtIndex(i).FindPropertyRelative(CELLS_PROPERTY_NAME).arraySize = correctSize;
                }

                return correctSize;
            }

            public override void ValidateLevel() // validation from old editor
            {
                errorLabels.Clear();
                //check layers size
                for (int i = 0; i < layersProperty.arraySize; i++)
                {
                    if (GetLayerSize(i) != firstLayerSizeProperty.intValue + (i % 2))
                    {
                        errorLabels.Add(GetLayerName(i) + " have incorrect size. Correct size: " + (firstLayerSizeProperty.intValue + (i % 2)));
                    }
                }

                //check for element

                int elementCounter = 0;
                int layerSize;

                for (int i = 0; i < layersProperty.arraySize; i++)
                {
                    layerSize = GetLayerSize(i);

                    for (int x = 0; x < layerSize; x++)
                    {
                        for (int y = 0; y < layerSize; y++)
                        {
                            if (GetCellValue(i, x, y))
                            {
                                elementCounter++;
                            }
                        }
                    }
                }

                if (elementCounter % 3 != 0)
                {
                    errorLabels.Add("Incorrect number of selected cells.Cells % 3 =" + (elementCounter % 3));
                }
            }

            public int GetLayerSize(int layerIndex)
            {
                return layersProperty.GetArrayElementAtIndex(layerIndex).FindPropertyRelative(ROWS_PROPERTY_NAME).arraySize;
            }

            public string GetLayerName(int layerIndex)
            {
                return String.Format("Layer # {0} ({1}x{1})", layerIndex + 1, GetLayerSize(layerIndex));
            }

            public void AddLayer()
            {
                layersProperty.arraySize++;

                int layerIndex = layersProperty.arraySize - 1;
                SetCorrectLevelSize(layerIndex);
                int layerSize = GetLayerSize(layerIndex);

                for (int i = 0; i < layerSize; i++)
                {
                    for (int j = 0; j < layerSize; j++)
                    {
                        SetCellValue(layerIndex, i, j, false);
                    }
                }

            }
        }
    }
}

// -----------------
// 2d grid level editor
// -----------------

// Changelog
// v 1.2
// • Reordered some methods
// v 1.1
// • Added global validation
// • Added validation example
// • Fixed mouse click bug
// v 1 basic version works