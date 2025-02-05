using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using static Watermelon.ProjectCleanUpTool.EditorTable;

namespace Watermelon.ProjectCleanUpTool
{
    public class CleanUpToolWindow : EditorWindow
    {
        private static CleanUpToolWindow window;
        private const string ASSETS_FOLDER = "Assets/";
        private const char PATH_SEPARATOR = '/';
        private const string PROGRESS_BAR_TITLE_DURING_SCAN = "Scan progress";
        private Vector2 scrollVector;
        //general
        int selectedTabIndex = 0;
        string[] tabNames = { "Settings","File info table","Asset organizer" };
        string[] categories = { "All","InBuild","NotInbuild", "Used", "Unused"};
        private string lastScanLabel;
        private EditorTable table;
        private string[] extensions;
        private string[] extensionFolders;
        private List<DisplayFile> displayFiles;
        private Vector2 settingsTabScrollVector;
        private List<MisplacedFile> misplacedFiles;
        private Vector2 assetOrganizerScrollVector;
        private int misplacedFileIndex;
        private int selectedCategory;

        [MenuItem("Tools/Project cleaner")]
        static void ShowWindow()
        {
            if (window != null)
            {
                window.Close();
            }

            window = (CleanUpToolWindow)EditorWindow.GetWindow(typeof(CleanUpToolWindow));
            window.titleContent = new GUIContent("Project cleaner");
            window.minSize = new Vector2(700, 600);
            window.Show();

        }

        protected void OnEnable()
        {
            misplacedFiles = new List<MisplacedFile>();

            if (DatabaseHandler.Instance.NullDatabase || DatabaseHandler.Instance.EmptyDatabase)
            {
                lastScanLabel = "Last scan dateTime is unknown because database is empty.";
            }
            else
            {
                lastScanLabel = "Last scan :" + new DateTime(DatabaseHandler.Instance.LastScanDateInTics);
                displayFiles = DatabaseHandler.Instance.LoadDisplayFiles();
                table = new EditorTable(displayFiles, DatabaseHandler.Instance.LoadTypeTextures());
                extensions = DatabaseHandler.Instance.GetFileTypeExtensions();
                
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            //scrollVector = EditorGUILayout.BeginScrollView(scrollVector);
            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, tabNames);

            switch (selectedTabIndex)
            {
                case 1:
                    DrawFileUseTab();
                    break;
                case 2:
                    DrawAssetOrganizerTab();
                    break;
                case 0:
                default:
                    //DrawDataCollectionTab();
                    DrawSettingsTab();
                    break;
            }

            //EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawSettingsTab()
        {
            
            settingsTabScrollVector = EditorGUILayout.BeginScrollView(settingsTabScrollVector);
            EditorGUILayout.HelpBox("Intruction:", MessageType.Info);
            EditorGUILayout.HelpBox("Step #1. Click \"Select files to scan\" and select files you want to scan. (Everything beside different Api`s or Watermelon Core folder)",MessageType.Info);
            EditorGUILayout.HelpBox("Step #2. Make sure \"scan during build\" is enabled, close this tool window and build project. ", MessageType.Info);
            EditorGUILayout.HelpBox("Step #3. Open \"File info table\" if you want to see where files is used and remove unused files.", MessageType.Info);
            EditorGUILayout.HelpBox("Step #4. Edit extension folders for step#5.", MessageType.Info);
            EditorGUILayout.HelpBox("Step #5. Open \"Asset organizer\" if you want to organize files according to extension folders.", MessageType.Info);
            EditorGUILayout.HelpBox("Step #6. Click \"Clear database\" and copy this asset to use in different project.", MessageType.Info);
            
            if (GUILayout.Button("Select files to scan"))
            {
                FileTreeWindow.OpenWindow(ASSETS_FOLDER, "Mark which files should be inspected", DatabaseHandler.Instance.GetScannedAssetPaths(), HandleFileTreeWindowResults);
            }

            DatabaseHandler.Instance.ConnectToBuild = EditorGUILayout.Toggle("scan during build", DatabaseHandler.Instance.ConnectToBuild);
            EditorGUILayout.LabelField(lastScanLabel);

            if (GUILayout.Button("Clear database"))
            {
                DatabaseHandler.Instance.ClearDatabase();
                lastScanLabel = "Last scan dateTime is unknown because database is empty.";
            }

            if ((DatabaseHandler.Instance.NullDatabase || DatabaseHandler.Instance.EmptyDatabase))
            {
                EditorGUILayout.EndScrollView();
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Extension folders:");

            for (int i = 0; i < extensions.Length; i++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Extension:  " + extensions[i]);
                
                if(GUILayout.Button("Add"))
                {
                    DatabaseHandler.Instance.AddFolder(i, OpenFolderForExtension(extensions[i]));
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();

                extensionFolders = DatabaseHandler.Instance.GetExtensionFolders(i);

                for (int j = 0; j < extensionFolders.Length; j++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Folder: " + extensionFolders[j]);

                    if (GUILayout.Button("Edit"))
                    {
                        DatabaseHandler.Instance.AddFolder(i, OpenFolderForExtension(extensions[i],extensionFolders[j]));
                    }

                    if (GUILayout.Button("Remove"))
                    {
                        DatabaseHandler.Instance.RemoveFolder(i, j);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
        }

        private string OpenFolderForExtension(string extension,string folder = ASSETS_FOLDER)
        {
            string result = EditorUtility.OpenFolderPanel("Folder for extension: " + extension, folder, string.Empty);

            if (result.Equals(string.Empty))
            {
                return string.Empty;
            }

            return ASSETS_FOLDER + result.Replace(Application.dataPath, string.Empty).Remove(0, 1);
        }

        private void DrawFileUseTab()
        {
            if (DatabaseHandler.Instance.NullDatabase || DatabaseHandler.Instance.EmptyDatabase)
            {
                EditorGUILayout.LabelField("Can`t display table without data from database");
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Categories:");

            EditorGUI.BeginChangeCheck();
            selectedCategory = GUILayout.Toolbar(selectedCategory, categories);

            if (EditorGUI.EndChangeCheck())
            {
                switch (selectedCategory)
                {
                    case 0:
                        table.SetCategory(Category.NoLimits);
                        break;
                    case 1:
                        table.SetCategory(Category.InBuild);
                        break;
                    case 2:
                        table.SetCategory(Category.NotInBuild);
                        break;
                    case 3:
                        table.SetCategory(Category.Used);
                        break;
                    case 4:
                        table.SetCategory(Category.Unused);
                        break;

                }
            }


            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Files:");

            if(table != null)
            {
                table.DrawTable();
            }
            
        }

        private void HandleFileTreeWindowResults(bool changesApplied, string[] assetPaths)
        {
            if (changesApplied)
            {
                DatabaseHandler.Instance.SetScannedAssetPaths(assetPaths);
            }
        }

        private void DrawAssetOrganizerTab()
        {
            if (DatabaseHandler.Instance.NullDatabase || DatabaseHandler.Instance.EmptyDatabase)
            {
                EditorGUILayout.LabelField("Can`t organize assets without data from database");
                return;
            }

            if (GUILayout.Button("Look for misplased assets"))
            {
                LookForMisplacedAssets();
            }

            if (misplacedFiles.Count == 0)
            {
                return;
            }

            assetOrganizerScrollVector = EditorGUILayout.BeginScrollView(assetOrganizerScrollVector);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField(misplacedFiles.Count + " misplaced files left");
            EditorGUILayout.LabelField("Editing asset:  " + misplacedFiles[misplacedFileIndex].path);

            EditorGUI.indentLevel++;
            
            for (int i = 0; (misplacedFileIndex < misplacedFiles.Count) && (i < misplacedFiles[misplacedFileIndex].suggestions.Length); i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(misplacedFiles[misplacedFileIndex].suggestions[i]);

                if (GUILayout.Button("Move", GUILayout.Width(50)))
                {
                    MoveAsset(misplacedFiles[misplacedFileIndex].path, misplacedFiles[misplacedFileIndex].suggestions[i]);
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(4);
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(misplacedFileIndex == 0);

            if (GUILayout.Button("Previous"))
            {
                misplacedFileIndex--;
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(misplacedFileIndex == misplacedFiles.Count - 1);

            if (GUILayout.Button("Skip"))
            {
                misplacedFileIndex++;
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Select"))
            {
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(misplacedFiles[misplacedFileIndex].path);
            }

            if (GUILayout.Button("Move manualy"))
            {
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(misplacedFiles[misplacedFileIndex].path);
                MoveAsset(misplacedFiles[misplacedFileIndex].path, OpenFolderForAsset(misplacedFiles[misplacedFileIndex].filename));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void MoveAsset(string oldPath, string newPath)
        {
            if (newPath.Equals(string.Empty))
            {
                return;
            }

            string validationResult = AssetDatabase.ValidateMoveAsset(oldPath, newPath);

            if (validationResult.Equals(string.Empty))
            {
                misplacedFiles.RemoveAt(misplacedFileIndex);

                if (misplacedFileIndex >= misplacedFiles.Count)
                {
                    misplacedFileIndex = 0;
                }

                AssetDatabase.MoveAsset(oldPath, newPath);
            }
            else
            {
                Debug.LogError(validationResult);
            }
        }

        private string OpenFolderForAsset(string filename)
        {
            string result = EditorUtility.OpenFolderPanel("Folder for asset: " + filename, ASSETS_FOLDER, string.Empty);

            if (result.Equals(string.Empty))
            {
                return string.Empty;
            }

            return ASSETS_FOLDER + result.Replace(Application.dataPath, string.Empty).Remove(0, 1) + PATH_SEPARATOR + filename;
        }

        private void LookForMisplacedAssets()
        {
            misplacedFiles.Clear();
            List<string> suggestions = new List<string>();
            string[] subfoldersLevel1;
            string[] subfoldersLevel2;
            bool misplaced;

            for (int i = 0; i < displayFiles.Count; i++)
            {
                EditorUtility.DisplayProgressBar(PROGRESS_BAR_TITLE_DURING_SCAN, string.Empty, (i + 0f) / displayFiles.Count);
                extensionFolders = DatabaseHandler.Instance.GetExtensionFolders(displayFiles[i].textureIndex);

                if (extensionFolders.Length == 0)
                {
                    continue;
                }

                string path = AssetDatabase.GUIDToAssetPath(displayFiles[i].guid);
                misplaced = true;

                for (int j = 0; (misplaced) && (j < extensionFolders.Length); j++)
                {
                    if (path.Contains(extensionFolders[j]))
                    {
                        misplaced = false;
                    }
                }

                if (misplaced)
                {
                    suggestions.Clear();

                    for (int j = 0; j < extensionFolders.Length; j++)
                    {
                        suggestions.Add(extensionFolders[j] + PATH_SEPARATOR + displayFiles[i].filename);
                        subfoldersLevel1 = AssetDatabase.GetSubFolders(extensionFolders[j]);

                        for (int subfolder1Index = 0; subfolder1Index < subfoldersLevel1.Length; subfolder1Index++)
                        {
                            subfoldersLevel2 = AssetDatabase.GetSubFolders(subfoldersLevel1[subfolder1Index]);
                            suggestions.Add(subfoldersLevel1[subfolder1Index] + PATH_SEPARATOR + displayFiles[i].filename);

                            for (int subfolder2Index = 0; subfolder2Index < subfoldersLevel2.Length; subfolder2Index++)
                            {
                                suggestions.Add(subfoldersLevel2[subfolder2Index] + PATH_SEPARATOR + displayFiles[i].filename);
                            }
                        }
                    }

                    

                    misplacedFiles.Add(new MisplacedFile(path, displayFiles[i].filename, suggestions.ToArray()));
                }
            }

            if(misplacedFiles.Count == 0)
            {
                EditorUtility.DisplayDialog("Info", "All assets are organized.", "Ok");
            }

            misplacedFileIndex = 0;
            EditorUtility.ClearProgressBar();
        }

        private class MisplacedFile
        {
            public string path;
            public string filename;
            public string[] suggestions;

            public MisplacedFile(string path, string filename, string[] suggestions)
            {
                this.path = path;
                this.filename = filename;
                this.suggestions = suggestions;
            }
        }
    }
}