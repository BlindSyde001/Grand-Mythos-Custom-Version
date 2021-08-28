using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEngine;

public static class SaveManager
{
    public static void SaveToFile(SaveData saveData, string newFileName)
    {
        if(!Directory.Exists(Application.persistentDataPath + "/Save Files")) // Creates the Folder for Save files if there isn't one already
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Save Files");
        }

        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(Application.persistentDataPath + "/Save Files/" + newFileName + ".json", json);
    }
    public static SaveData LoadFromFile(string fileName)
    {
        string json = "";
        SaveData loadData = new SaveData();
        try
        {
            json = File.ReadAllText(Application.persistentDataPath + "/Save Files/" + fileName + ".json");
            loadData = JsonUtility.FromJson<SaveData>(json);
        }
        catch
        {
            Debug.LogError("Warning, could not load the file!");
        }
        return loadData;
    }
}
