using UnityEngine;

[System.Serializable]
public class SavedDataContainer
{
    [SerializeField] string containerUniqueName;
    public string ContainerUniqueName => containerUniqueName;

    [SerializeField] ISaveObject saveObject;
    public ISaveObject SaveObject => saveObject;

    public SavedDataContainer(ISaveController saveController)
    {
        containerUniqueName = saveController.UniqueSaveName;
        saveObject = saveController.Save();
    }
}
