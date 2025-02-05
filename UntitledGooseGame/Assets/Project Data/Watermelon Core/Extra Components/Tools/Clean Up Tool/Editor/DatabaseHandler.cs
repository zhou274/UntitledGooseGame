using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Watermelon.ProjectCleanUpTool;
using System;
using static Watermelon.ProjectCleanUpTool.EditorTable;

namespace Watermelon.ProjectCleanUpTool
{
    public class DatabaseHandler
    {
        private static DatabaseHandler instance;

        SerializedObject databaseSerializedObject;
        CleanUpToolDatabase database;
        private bool nullDatabase;
        private string projectFolderPath = Application.dataPath.Replace("Assets", string.Empty);

        //FileDependancyDatabase property names
        private const string FILES_PROPERTY_NAME = "fileInfos";
        private const string NODES_PROPERTY_NAME = "nodes";
        private const string DATA_COLLECTING_TIMESTAMP_PROPERTY_NAME = "dataCollectingTimestamp";
        private const string CONNECT_TO_BUILD_PROPERTY_NAME = "connectToBuild";
        private const string SCANNED_ASSET_PATHS_PROPERTY_NAME = "scannedAssetPaths";
        //TypesDatabase property names
        private const string TYPES_PROPERTY_NAME = "types";

        //FileInfo
        private const string GUID_PROPERTY_NAME = "guid";
        private const string FILENAME_PROPERTY_NAME = "filename";
        private const string FILE_TYPE_PROPERTY_NAME = "fileType";
        private const string DISK_SIZE_PROPERTY_NAME = "diskSize";
        private const string BUILD_SIZE_PROPERTY_NAME = "buildSize";
        private const string DELETED_PROPERTY_NAME = "deleted";
        private const string USED_IN_BUILD_PROPERTY_NAME = "usedInBuild";

        private const string NODE_INDEXES_PROPERTY_NAME = "nodeIndexes";
        //Node
        private const string ROOT_FILE_INDEX_PROPERTY_NAME = "rootFileIndex";
        private const string PARENT_INDEX_PROPERTY_NAME = "parentIndex";
        private const string FILE_INDEX_PROPERTY_NAME = "fileIndex";
        //FileType
        private const string EXTENSION_PROPERTY_NAME = "extension";
        private const string TYPE_PROPERTY_NAME = "type";
        private const string FOLDERS_PROPERTY_NAME = "folders";
        private const string UNKNOWN = "-";


        //database properties
        private SerializedProperty fileInfosProperty;
        private SerializedProperty nodesProperty;
        private SerializedProperty dataCollectingTimestampProperty;
        private SerializedProperty typesProperty;
        private SerializedProperty connectToBuildProperty;
        private SerializedProperty scannedAssetPathsProperty;
        
        //temp properties
        private SerializedProperty fileProperty;
        private SerializedProperty newElementProperty;
        private SerializedProperty childNodesProperty;
        private SerializedProperty nodeIndexesProperty;
        private SerializedProperty foldersProperty;

        public static DatabaseHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DatabaseHandler();
                }

                return instance;
            }
        }

        public bool NullDatabase { get => nullDatabase; }

        public bool EmptyDatabase { get => dataCollectingTimestampProperty.longValue == -1; }

        public long LastScanDateInTics { get => dataCollectingTimestampProperty.longValue;  }

        public bool ConnectToBuild { get => connectToBuildProperty.boolValue; set => connectToBuildProperty.boolValue = value; }

        public DatabaseHandler()
        {
            database = GetDatabase();

            if (database == null)
            {
                nullDatabase = true;
            }
            else
            {
                nullDatabase = false;
                databaseSerializedObject = new SerializedObject(database);
                fileInfosProperty = databaseSerializedObject.FindProperty(FILES_PROPERTY_NAME);
                nodesProperty = databaseSerializedObject.FindProperty(NODES_PROPERTY_NAME);
                dataCollectingTimestampProperty = databaseSerializedObject.FindProperty(DATA_COLLECTING_TIMESTAMP_PROPERTY_NAME);
                typesProperty = databaseSerializedObject.FindProperty(TYPES_PROPERTY_NAME);
                connectToBuildProperty = databaseSerializedObject.FindProperty(CONNECT_TO_BUILD_PROPERTY_NAME);
                scannedAssetPathsProperty = databaseSerializedObject.FindProperty(SCANNED_ASSET_PATHS_PROPERTY_NAME);
            }
        }

        private CleanUpToolDatabase GetDatabase()
        {
            string[] searchResult = AssetDatabase.FindAssets("t:CleanUpToolDatabase");
            
            if(searchResult.Length > 0)
            {
                return AssetDatabase.LoadAssetAtPath<CleanUpToolDatabase>(AssetDatabase.GUIDToAssetPath(searchResult[0]));
            }

            return null;
        }

        public void ClearDatabase()
        {
            ClearScanData();
            ClearSettings();
            databaseSerializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        public void ClearScanData()
        {
            fileInfosProperty.arraySize = 0;
            nodesProperty.arraySize = 0;
            dataCollectingTimestampProperty.longValue = -1;
            databaseSerializedObject.ApplyModifiedProperties();
        }

        public void ClearSettings()
        {
            scannedAssetPathsProperty.arraySize = 0;
            typesProperty.arraySize = 0;
            ConnectToBuild = false;
            databaseSerializedObject.ApplyModifiedProperties();
        }

        public void ApplyChanges()
        {
            databaseSerializedObject.ApplyModifiedProperties();
        }

        public void SetScannedAssetPaths(string[] assetPaths)
        {
            scannedAssetPathsProperty.arraySize = assetPaths.Length;

            for (int i = 0; i < assetPaths.Length; i++)
            {
                scannedAssetPathsProperty.GetArrayElementAtIndex(i).stringValue = assetPaths[i];
            }

            databaseSerializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        public List<string> GetScannedAssetPaths()
        {
            List<string> result = new List<string>();

            for (int i = 0; i < scannedAssetPathsProperty.arraySize; i++)
            {
                result.Add(scannedAssetPathsProperty.GetArrayElementAtIndex(i).stringValue);
            }

            return result;
        }

        public string[] GetFileTypeExtensions()
        {
            string[] result = new string[typesProperty.arraySize];

            for (int i = 0; i < typesProperty.arraySize; i++)
            {
                result[i] = typesProperty.GetArrayElementAtIndex(i).FindPropertyRelative(EXTENSION_PROPERTY_NAME).stringValue;
            }

            return result;
        }

        #region Used by Clean Up Tool Window

        public string[] GetExtensionFolders(int extensionIndex)
        {
            foldersProperty = typesProperty.GetArrayElementAtIndex(extensionIndex).FindPropertyRelative(FOLDERS_PROPERTY_NAME);
            string[] result = new string[foldersProperty.arraySize];

            for (int i = 0; i < foldersProperty.arraySize; i++)
            {
                result[i] = foldersProperty.GetArrayElementAtIndex(i).stringValue;
            }

            return result;
        }

        public void AddFolder(int extensionIndex, string folder)
        {
            if (folder.Equals(string.Empty))
            {
                return;
            }

            foldersProperty = typesProperty.GetArrayElementAtIndex(extensionIndex).FindPropertyRelative(FOLDERS_PROPERTY_NAME);
            foldersProperty.arraySize++;
            foldersProperty.GetArrayElementAtIndex(foldersProperty.arraySize - 1).stringValue = folder;
            databaseSerializedObject.ApplyModifiedProperties();
        }

        public void AddFolder(int extensionIndex, int folderIndex, string folder)
        {
            if (folder.Equals(string.Empty))
            {
                return;
            }

            foldersProperty = typesProperty.GetArrayElementAtIndex(extensionIndex).FindPropertyRelative(FOLDERS_PROPERTY_NAME);
            foldersProperty.GetArrayElementAtIndex(folderIndex).stringValue = folder;
            databaseSerializedObject.ApplyModifiedProperties();
        }

        public void RemoveFolder(int extensionIndex, int folderIndex)
        {
            foldersProperty = typesProperty.GetArrayElementAtIndex(extensionIndex).FindPropertyRelative(FOLDERS_PROPERTY_NAME);
            foldersProperty.DeleteArrayElementAtIndex(folderIndex);
            databaseSerializedObject.ApplyModifiedProperties();
        }





        public List<Texture> LoadTypeTextures()
        {
            List<Texture> typeTextures = new List<Texture>();
            Type type;

            for (int i = 0; i < typesProperty.arraySize; i++)
            {
                type = Type.GetType(typesProperty.GetArrayElementAtIndex(i).FindPropertyRelative(TYPE_PROPERTY_NAME).stringValue);
                typeTextures.Add(EditorGUIUtility.ObjectContent(null, type).image);
            }

            return typeTextures;
        }

        public List<DisplayFile> LoadDisplayFiles()
        {
            List<DisplayFile> displayFiles = new List<DisplayFile>();
            SerializedProperty fileInfoProperty;
            DisplayFile displayFile;

            for (int i = 0; i < fileInfosProperty.arraySize; i++)
            {
                fileInfoProperty = fileInfosProperty.GetArrayElementAtIndex(i);

                if (fileInfoProperty.FindPropertyRelative(DELETED_PROPERTY_NAME).boolValue)
                {
                    continue;
                }

                //file deleted outside window
                if (!System.IO.File.Exists(projectFolderPath + AssetDatabase.GUIDToAssetPath(fileInfoProperty.FindPropertyRelative(GUID_PROPERTY_NAME).stringValue)))
                {
                    ProcessDeleteFile(i);
                    continue;
                }

                displayFile = new DisplayFile();
                displayFile.originalIndex = i;
                displayFile.filename = fileInfoProperty.FindPropertyRelative(FILENAME_PROPERTY_NAME).stringValue;
                displayFile.guid = fileInfoProperty.FindPropertyRelative(GUID_PROPERTY_NAME).stringValue;
                displayFile.textureIndex = fileInfoProperty.FindPropertyRelative(FILE_TYPE_PROPERTY_NAME).intValue;
                displayFile.usages = fileInfoProperty.FindPropertyRelative(NODE_INDEXES_PROPERTY_NAME).arraySize;
                displayFile.diskSize = fileInfoProperty.FindPropertyRelative(DISK_SIZE_PROPERTY_NAME).stringValue;
                displayFile.buildSize = fileInfoProperty.FindPropertyRelative(BUILD_SIZE_PROPERTY_NAME).stringValue;
                displayFile.usedInBuild = fileInfoProperty.FindPropertyRelative(USED_IN_BUILD_PROPERTY_NAME).boolValue;

                //handle build size

                if (displayFile.usedInBuild)
                {
                    displayFile.usedInBuildString = UNKNOWN;
                }
                else
                {
                    displayFile.usedInBuildString = "+";
                }

                displayFiles.Add(displayFile);
            }

            return displayFiles;
        }

        private static string GetSizeAsString(long byteSize)
        {
            string sizeAsString = String.Empty;

            float b = byteSize;
            float kb = b / 1024f;
            float mb = kb / 1024f;
            float gb = mb / 1024f;

            if (gb >= 1)
            {
                sizeAsString = String.Format(((float)Math.Round(gb, 1)).ToString(), "0.00") + " gb";
            }
            else if (mb >= 1)
            {
                sizeAsString = String.Format(((float)Math.Round(mb, 1)).ToString(), "0.00") + " mb";
            }
            else if (kb >= 1)
            {
                sizeAsString = String.Format(((float)Math.Round(kb, 1)).ToString(), "0.00") + " kb";
            }
            else if (byteSize >= 0)
            {
                sizeAsString = String.Format(((float)Math.Round(b, 1)).ToString(), "0.00") + " b";
            }
            return sizeAsString;
        }

        

        public void ProcessDeleteFile(int index)
        {
            fileProperty = fileInfosProperty.GetArrayElementAtIndex(index);
            fileProperty.FindPropertyRelative(DELETED_PROPERTY_NAME).boolValue = true;
            AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(fileProperty.FindPropertyRelative(GUID_PROPERTY_NAME).stringValue));
            databaseSerializedObject.ApplyModifiedProperties();
        }

        public int UpdateUsages(int fileIndex)
        {
            return fileInfosProperty.GetArrayElementAtIndex(fileIndex).FindPropertyRelative(NODE_INDEXES_PROPERTY_NAME).arraySize;
        }

        public string GetUsagesPaths(int fileIndex)
        {
            nodeIndexesProperty = fileInfosProperty.GetArrayElementAtIndex(fileIndex).FindPropertyRelative(NODE_INDEXES_PROPERTY_NAME);

            if (nodeIndexesProperty.arraySize == 0)
            {
                return string.Empty;
            }
            else if (nodeIndexesProperty.arraySize == 1)
            {
                return GetUsageFromNode(nodesProperty.GetArrayElementAtIndex(nodeIndexesProperty.GetArrayElementAtIndex(0).intValue));
            }
            else
            {
                string[] usages = new string[nodeIndexesProperty.arraySize];

                for (int i = 0; i < usages.Length; i++)
                {
                    usages[i] = GetUsageFromNode(nodesProperty.GetArrayElementAtIndex(nodeIndexesProperty.GetArrayElementAtIndex(i).intValue));
                }

                return string.Join("\n", usages);
            }
        }

        private string GetUsageFromNode(SerializedProperty nodeProperty)
        {
            if(nodeProperty.FindPropertyRelative(PARENT_INDEX_PROPERTY_NAME).intValue == -1)// means it`s scene
            {
                fileProperty = fileInfosProperty.GetArrayElementAtIndex(nodeProperty.FindPropertyRelative(ROOT_FILE_INDEX_PROPERTY_NAME).intValue);
            }
            else
            {
                fileProperty = fileInfosProperty.GetArrayElementAtIndex(nodeProperty.FindPropertyRelative(PARENT_INDEX_PROPERTY_NAME).intValue);
            }

            return AssetDatabase.GUIDToAssetPath(fileProperty.FindPropertyRelative(GUID_PROPERTY_NAME).stringValue);
        }

        #endregion

        #region used by build processor

        public void AddFileType(Type type, string extension)
        {
            typesProperty.arraySize++;
            newElementProperty = typesProperty.GetArrayElementAtIndex(typesProperty.arraySize - 1);
            newElementProperty.FindPropertyRelative(EXTENSION_PROPERTY_NAME).stringValue = extension;
            newElementProperty.FindPropertyRelative(TYPE_PROPERTY_NAME).stringValue = type.AssemblyQualifiedName;
            newElementProperty.FindPropertyRelative(FOLDERS_PROPERTY_NAME).arraySize = 0;
        }

        public void AddFileInfo(string assetPath, string filename, int fileTypeIndex, long diskSize)
        {
            fileInfosProperty.arraySize++;
            newElementProperty = fileInfosProperty.GetArrayElementAtIndex(fileInfosProperty.arraySize - 1);
            newElementProperty.FindPropertyRelative(GUID_PROPERTY_NAME).stringValue = AssetDatabase.AssetPathToGUID(assetPath);
            newElementProperty.FindPropertyRelative(FILENAME_PROPERTY_NAME).stringValue = filename;
            newElementProperty.FindPropertyRelative(FILE_TYPE_PROPERTY_NAME).intValue = fileTypeIndex;
            newElementProperty.FindPropertyRelative(DISK_SIZE_PROPERTY_NAME).stringValue = GetSizeAsString(diskSize);
            newElementProperty.FindPropertyRelative(BUILD_SIZE_PROPERTY_NAME).stringValue = UNKNOWN;
            newElementProperty.FindPropertyRelative(DELETED_PROPERTY_NAME).boolValue = false;
            newElementProperty.FindPropertyRelative(USED_IN_BUILD_PROPERTY_NAME).boolValue = false;
            newElementProperty.FindPropertyRelative(NODE_INDEXES_PROPERTY_NAME).arraySize = 0;
        }

        public void ClearNodes()
        {
            nodesProperty.arraySize = 0;
        }

        public void AddNodeIndex(int fileInfoIndex, int nodeIndex)
        {
            nodeIndexesProperty = fileInfosProperty.GetArrayElementAtIndex(fileInfoIndex).FindPropertyRelative(NODE_INDEXES_PROPERTY_NAME);
            nodeIndexesProperty.arraySize++;
            newElementProperty = nodeIndexesProperty.GetArrayElementAtIndex(nodeIndexesProperty.arraySize - 1);
            newElementProperty.intValue = nodeIndex;
        }

        public void UpdateBuildData(int fileInfoIndex, string size)
        {
            fileInfosProperty.GetArrayElementAtIndex(fileInfoIndex).FindPropertyRelative(BUILD_SIZE_PROPERTY_NAME).stringValue = size;
            fileInfosProperty.GetArrayElementAtIndex(fileInfoIndex).FindPropertyRelative(USED_IN_BUILD_PROPERTY_NAME).boolValue = true;
        }

        public void MarkUsedInBuildScene(int fileInfoIndex)
        {
            fileInfosProperty.GetArrayElementAtIndex(fileInfoIndex).FindPropertyRelative(USED_IN_BUILD_PROPERTY_NAME).boolValue = true;
        }

        public void AddNode(int rootFileIndex, int parentIndex, int fileIndex)
        {
            nodesProperty.arraySize++;
            newElementProperty = nodesProperty.GetArrayElementAtIndex(nodesProperty.arraySize - 1);
            newElementProperty.FindPropertyRelative(ROOT_FILE_INDEX_PROPERTY_NAME).intValue = rootFileIndex;
            newElementProperty.FindPropertyRelative(PARENT_INDEX_PROPERTY_NAME).intValue = parentIndex;
            newElementProperty.FindPropertyRelative(FILE_INDEX_PROPERTY_NAME).intValue = fileIndex;
        }

        public void SaveDatabase()
        {
            dataCollectingTimestampProperty.longValue = DateTime.Now.Ticks;
            databaseSerializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        #endregion
    }
}