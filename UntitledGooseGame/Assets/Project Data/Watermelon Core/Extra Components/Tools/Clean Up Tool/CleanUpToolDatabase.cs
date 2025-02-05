using UnityEngine;
using UnityEngine.SceneManagement;

namespace Watermelon.ProjectCleanUpTool
{
    [CreateAssetMenu(fileName = "CleanUpToolDatabase", menuName = "Tools/CleanUpToolDatabase")]
    public class CleanUpToolDatabase : ScriptableObject
    {
        [SerializeField] private FileInfo[] fileInfos;
        [SerializeField] private Node[] nodes;
        [SerializeField] private FileType[] types;
        [SerializeField] private long dataCollectingTimestamp; // default -1 means data not collected
        [SerializeField] private bool connectToBuild;
        [SerializeField] private string[] scannedAssetPaths;

        [System.Serializable]
        public class FileInfo
        {
            [SerializeField] private string filename;
            [SerializeField] private string guid;
            [SerializeField] private int fileType;
            [SerializeField] private string diskSize;
            [SerializeField] private string buildSize;
            [SerializeField] private bool deleted;
            [SerializeField] private bool usedInBuild;
            [SerializeField] private int[] nodeIndexes;
        }

        [System.Serializable]
        public class Node
        {
            [SerializeField] private int rootFileIndex;
            [SerializeField] private int parentIndex;
            [SerializeField] private int fileIndex;
        }

        [System.Serializable]
        public class FileType
        {
            [SerializeField] private string extension;
            [SerializeField] private string type;
            [SerializeField] private string[] folders;
        }
    }
}