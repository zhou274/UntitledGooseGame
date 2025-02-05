using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using Watermelon;
using System.IO;
using UnityEditor.UnityLinker;
using UnityEditor.Callbacks;

namespace Watermelon.ProjectCleanUpTool
{
    public class BuildProcessor : IPreprocessBuildWithReport, IProcessSceneWithReport
    {
        private const string SCENE_EXTENSION = ".unity";
        private const string DEFAULT_EXTENSION = ".unknown";

        //log file parse
        private const string START_LINE = "Used Assets and files from the Resources folder, sorted by uncompressed size:";
        private const string FINISH_LINE = "-------------------------------------------------------------------------------";
        private const string BUILD_FINISHED = "Build completed with a result of";
        private const char PERCENT_CHAR = '%';
        private const char TAB_CHAR = '\t';
        private const string LOG_FILE_RELATIVE_PATH = "\\Unity\\Editor\\Editor.log";

        public int deletedFileCounter;
        public int fileCounter;
        public int buildOnlyFileCounter;

        //lists used to optimize collecting data during build
        private static List<string> assetPaths;
        private static List<int> sceneIndexes;
        private static List<string> fileNames;
        private static List<string> extensions;
        private static List<NodeInformation> nodeInformation;
        
        private int tempInt;
        private long tempLong;
        private string projectFolderPath = Application.dataPath.Replace("Assets", string.Empty);


        int IOrderedCallback.callbackOrder => int.MaxValue;

        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
            if (DatabaseHandler.Instance.NullDatabase || (!DatabaseHandler.Instance.ConnectToBuild))
            {
                return;
            }

            DatabaseHandler.Instance.ClearScanData();
            CollectAssetPaths();
            CollectExtensions();
            CollectFileInfos();
            nodeInformation = new List<NodeInformation>(); //used for optimization
            Application.logMessageReceived += HandleLog; // used to find out when build is  finally finished

            foreach (int sceneIndex in sceneIndexes)
            {
                CollectNodes(sceneIndex);
            }
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (logString.Contains(BUILD_FINISHED))
            {
                Application.logMessageReceived -= HandleLog;
                OnBuildFinished();
            }
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (DatabaseHandler.Instance.NullDatabase || (!DatabaseHandler.Instance.ConnectToBuild) || (!BuildPipeline.isBuildingPlayer))
            {
                return;
            }

            CollectNodes(assetPaths.IndexOf(scene.path), true);
        }

        public void OnBuildFinished()
        {
            AddNodesToDatabase();
            ParseEditorLogFile();
            DatabaseHandler.Instance.SaveDatabase();
        }

        private void CollectAssetPaths()
        {
            //Get all assets
            assetPaths = DatabaseHandler.Instance.GetScannedAssetPaths();

            for (int i = assetPaths.Count - 1; i >= 0; i--)
            {
                if (AssetDatabase.IsValidFolder(assetPaths[i]))//Remove folders
                {
                    assetPaths.RemoveAt(i);
                }
            }
        }

        private void CollectExtensions()
        {
            extensions = new List<string>();
            extensions.AddRange(DatabaseHandler.Instance.GetFileTypeExtensions());
            Dictionary<string, int> newExtensionIndexes = new Dictionary<string, int>();

            //look for extension
            string extension;

            for (int i = 0; i < assetPaths.Count; i++)
            {
                extension = GetExtension(assetPaths[i]);

                if (!extensions.Contains(extension))
                {
                    newExtensionIndexes.Add(extension, i);
                    extensions.Add(extension);
                }
            }

            //add file types 
            int fileIndex;
            Type type;


            for (int i = 0; i < extensions.Count; i++)
            {
                if (newExtensionIndexes.TryGetValue(extensions[i], out fileIndex))
                {
                    type = AssetDatabase.GetMainAssetTypeAtPath(assetPaths[fileIndex]);
                    DatabaseHandler.Instance.AddFileType(type, extensions[i]);
                }
            }

            DatabaseHandler.Instance.ApplyChanges();
        }

        private void CollectFileInfos()
        {
            sceneIndexes = new List<int>();

            for (int i = 0; i < assetPaths.Count; i++)
            {
                DatabaseHandler.Instance.AddFileInfo(assetPaths[i], GetFileName(assetPaths[i]), GetFileType(assetPaths[i]), GetDiskSize(assetPaths[i]));

                if (assetPaths[i].EndsWith(SCENE_EXTENSION))
                {
                    sceneIndexes.Add(i);

                    Debug.Log($"Scene index: {i} name: {GetFileName(assetPaths[i])} path: {assetPaths[i]}");
                }
            }

            DatabaseHandler.Instance.ApplyChanges();
        }

        private void CollectNodes(int sceneIndex, bool fromOnProcessScene = false)
        {
            List<string> filePaths = new List<string>();
            List<int> fileIndexes = new List<int>();
            int tempIndex;
            int currentListIndex = 0;
            tempIndex = sceneIndex;
            filePaths.Add(assetPaths[sceneIndex]);
            fileIndexes.Add(tempIndex);

            string[] dependencies;
            NodeInformation tempNodeInformation;

            string title;

            string description = "Files to process : ";

            if (!fromOnProcessScene)
            {
                nodeInformation.Add(new NodeInformation(sceneIndex, sceneIndex, -1)); //add root node
                title = "CleanUp tool (before build) scan scene: " + GetFileName(assetPaths[sceneIndex]);
            }
            else
            {
                DatabaseHandler.Instance.MarkUsedInBuildScene(sceneIndex);
                DatabaseHandler.Instance.ApplyChanges();
                title = "CleanUp tool (during build) scan scene: " + GetFileName(assetPaths[sceneIndex]);
            }

            while (currentListIndex < fileIndexes.Count)
            {
                dependencies = AssetDatabase.GetDependencies(filePaths[currentListIndex]);

                for (int i = 0; i < dependencies.Length; i++)
                {
                    tempIndex = assetPaths.IndexOf(dependencies[i]);

                    EditorUtility.DisplayProgressBar(title, description + fileIndexes.Count.ToString(), (i + 0f) / fileIndexes.Count);

                    //ignore files otside of asset folder
                    if (tempIndex == -1)
                    {
                        continue;
                    }

                    //handle situation when file refers to itself 
                    if (tempIndex == fileIndexes[currentListIndex])
                    {
                        continue;
                    }

                    tempNodeInformation = new NodeInformation(tempIndex, sceneIndex, fileIndexes[currentListIndex]);

                    //ignore already existing nodes
                    if (fromOnProcessScene && nodeInformation.Contains(tempNodeInformation))
                    {
                        continue;
                    }

                    nodeInformation.Add(tempNodeInformation);

                    //handle situation when more than 1 file have the same dependency. Example: shader for 2 different materials
                    for (int j = fileIndexes.Count - 1; j >= 0; j--)
                    {
                        if (fileIndexes[j] == tempIndex)
                        {
                            continue;
                        }
                    }

                    filePaths.Add(dependencies[i]);
                    fileIndexes.Add(tempIndex);
                }

                currentListIndex++;
            }

            EditorUtility.ClearProgressBar();
        }

        private void AddNodesToDatabase()
        {
            nodeInformation.Sort();
            DatabaseHandler.Instance.ClearNodes();

            for (int i = 0; i < nodeInformation.Count; i++)
            {
                DatabaseHandler.Instance.AddNode(nodeInformation[i].rootIndex, nodeInformation[i].parentIndex, nodeInformation[i].fileIndex);
                DatabaseHandler.Instance.AddNodeIndex(nodeInformation[i].fileIndex, i);
            }
        }

        void ParseEditorLogFile()
        {
            //step 1 try to get path of logFile
            if (Application.platform != RuntimePlatform.WindowsEditor)
            {
                Debug.LogError("Tool only supported on Windows so far.");
                return;
            }

            string logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + LOG_FILE_RELATIVE_PATH;

            if (!File.Exists(logFilePath))
            {
                Debug.LogError("Log file don`t exist. Path: " + logFilePath);
                return;
            }

            //step 2 add important lines to parseLines list

            List<string> parseLines = new List<string>();
            bool addLines = false;
            string line = string.Empty;



            using (FileStream fileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    while (streamReader.Peek() > -1)
                    {
                        line = streamReader.ReadLine();

                        if (addLines)
                        {
                            if (line.Equals(FINISH_LINE))
                            {
                                addLines = false;
                            }
                            else
                            {
                                parseLines.Add(line);
                            }
                        }
                        else
                        {
                            if (line.Equals(START_LINE))
                            {
                                addLines = true;
                                parseLines.Clear(); // in case of several builds we need only last one
                            }
                        }
                    }
                }
            }

            if(parseLines.Count == 0)
            {
                Debug.LogError("parseLines.Count == 0. Build data missing.");
                return;
            }

            //step #3 Parse lines 
            
            string size;
            string path;
            int index;

            foreach (string parseLine in parseLines)
            {
                size = parseLine.Substring(0, parseLine.IndexOf(TAB_CHAR)).Replace('.', ',').Trim();
                path = parseLine.Substring(parseLine.IndexOf(PERCENT_CHAR) + 2);

                index = assetPaths.IndexOf(path);

                if (index != -1)
                {
                    DatabaseHandler.Instance.UpdateBuildData(index, size);
                }
            }
        }
        private string GetFileName(string assetPath)
        {
            return assetPath.Substring(assetPath.LastIndexOf('/') + 1);
        }

        private int GetFileType(string assetPath)
        {
            return extensions.IndexOf(GetExtension(assetPath));
        }

        private long GetDiskSize(string assetPath)
        {
            tempLong = 0;

            try
            {
                tempLong = new System.IO.FileInfo(projectFolderPath + assetPath).Length;
                return tempLong;
            }
            catch (System.IO.IOException e)
            {
                Debug.LogError(assetPath);
                Debug.LogError(e);
            }

            return 0;
        }

        private string GetExtension(string assetPath)
        {
            tempInt = assetPath.LastIndexOf('.');

            if (tempInt == -1)
            {
                Debug.LogWarning("File without extension: " + assetPath);
                return DEFAULT_EXTENSION;
            }

            return assetPath.Substring(tempInt);
        }

        private struct NodeInformation : IEquatable<NodeInformation>, IComparable<NodeInformation>
        {
            public int fileIndex;
            public int rootIndex;
            public int parentIndex;

            public NodeInformation(int fileIndex, int rootIndex, int parentIndex)
            {
                this.fileIndex = fileIndex;
                this.rootIndex = rootIndex;
                this.parentIndex = parentIndex;
            }

            public int CompareTo(NodeInformation other)
            {
                int tempValue;
                tempValue = rootIndex.CompareTo(other.rootIndex);

                if (tempValue != 0)
                {
                    return tempValue;
                }
                else
                {
                    tempValue = parentIndex.CompareTo(other.parentIndex);

                    if (tempValue != 0)
                    {
                        return tempValue;
                    }
                    else
                    {
                        return fileIndex.CompareTo(other.fileIndex);
                    }
                }
            }

            public bool Equals(NodeInformation other)
            {
                return (fileIndex == other.fileIndex) && (rootIndex == other.rootIndex) && (parentIndex == other.parentIndex);
            }
        }

    }
}