using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;

public static class SaveController
{
    private const Serializer.SerializeType SAVE_SERIALIZE_TYPE = Serializer.SerializeType.Binary;
    private const string SAVE_FILE_NAME = "save";
    private const int SAVE_DELAY = 30;

    private static GlobalSave globalSave;

    private static List<ISaveController> saveObjects;
    private static int saveObjectsCount = 0;

    private static bool isSaveLoaded;
    private static bool isSaveRequired;

    public static void Initialise()
    {
        // Initialise save objects list
        saveObjects = new List<ISaveController>();
        saveObjectsCount = 0;

        // Load game save
        Load();

        // Enable auto-save coroutine
        Tween.InvokeCoroutine(AutoSaveCoroutine());
    }

    public static void RegisterSaveObject(ISaveController saveObject)
    {
        // Add object to the list
        saveObjects.Add(saveObject);
        saveObjectsCount++;

        // Check if save already loaded
        if (isSaveLoaded)
        {
            bool saveFounded = false;

            // Find save with the same ID and load data
            if(globalSave.SaveObjects != null)
            {
                for (int i = 0; i < globalSave.SaveObjects.Length; i++)
                {
                    if (globalSave.SaveObjects[i].ContainerUniqueName == saveObject.UniqueSaveName)
                    {
                        saveObject.Load(globalSave.SaveObjects[i].SaveObject);

                        saveFounded = true;

                        break;
                    }
                }
            }

            // Call load method with null parametr (to initialise)
            if (!saveFounded)
                saveObject.Load(null);
        }
    }

    public static void Load()
    {
        if (isSaveLoaded)
            return;

        // Try to read and deserialize file or create new one
        globalSave = Serializer.DeserializeFromPDP<GlobalSave>(SAVE_FILE_NAME, SAVE_SERIALIZE_TYPE, logIfFileNotExists: false);

        Debug.Log("[Save Controller]: Save is loaded!");

        isSaveLoaded = true;

        if(globalSave.SaveObjects != null)
        {
            for (int i = 0; i < globalSave.SaveObjects.Length; i++)
            {
                for (int s = 0; s < saveObjectsCount; s++)
                {
                    if (globalSave.SaveObjects[i].ContainerUniqueName == saveObjects[s].UniqueSaveName)
                    {
                        saveObjects[s].Load(globalSave.SaveObjects[i].SaveObject);

                        break;
                    }
                }
            }
        }
        else
        {
            for (int s = 0; s < saveObjectsCount; s++)
            {
                saveObjects[s].Load(null);
            }
        }
    }

    public static void Save()
    {
        if (!isSaveRequired)
            return;

        // Save registered objects
        SavedDataContainer[] saveDataContainers = new SavedDataContainer[saveObjectsCount];
        for (int i = 0; i < saveObjectsCount; i++)
        {
            saveDataContainers[i] = new SavedDataContainer(saveObjects[i]);
        }

        globalSave = new GlobalSave(saveDataContainers);

        Serializer.SerializeToPDP(globalSave, SAVE_FILE_NAME, SAVE_SERIALIZE_TYPE);

        Debug.Log("[Save Controller]: Game is saved!");

        isSaveRequired = false;
    }

    public static void ForceSave()
    {
        // Save registered objects
        SavedDataContainer[] saveDataContainers = new SavedDataContainer[saveObjectsCount];
        for (int i = 0; i < saveObjectsCount; i++)
        {
            saveDataContainers[i] = new SavedDataContainer(saveObjects[i]);
        }

        globalSave = new GlobalSave(saveDataContainers);

        Serializer.SerializeToPDP(globalSave, SAVE_FILE_NAME, SAVE_SERIALIZE_TYPE);

        Debug.Log("[Save Controller]: Game is saved!");

        isSaveRequired = false;
    }

    public static void MarkAsSaveIsRequired()
    {
        isSaveRequired = true;
    }

    private static IEnumerator AutoSaveCoroutine()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(SAVE_DELAY);

        while (true)
        {
            yield return waitForSeconds;

            Save();
        }
    }
}