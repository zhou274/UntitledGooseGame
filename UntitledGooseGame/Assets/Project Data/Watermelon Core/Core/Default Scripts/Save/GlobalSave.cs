using UnityEngine;

[System.Serializable]
public class GlobalSave
{
    [SerializeField] SavedDataContainer[] saveObjects;
    public SavedDataContainer[] SaveObjects => saveObjects;

    public GlobalSave()
    {
        saveObjects = null;
    }

    public GlobalSave(SavedDataContainer[] saveObjects)
    {
        this.saveObjects = saveObjects;
    }
}