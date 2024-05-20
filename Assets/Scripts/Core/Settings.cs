using System;
using System.IO;
using UnityEngine;

[Serializable]
public partial class Settings
{
    public static Settings Current { get; private set; } = new();

    public static string GetPath()
    {
        var exeDir = Path.GetDirectoryName(Application.dataPath);
        return Path.Combine(exeDir, "Settings.json");
    }

    public static void SaveToDisk()
    {
        try
        {
            var text = JsonUtility.ToJson(Current, true);
            File.WriteAllText(GetPath(), text);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public static bool TryLoadFromDisk()
    {
        try
        {
            if (Application.isEditor)
                return false;
            var text = File.ReadAllText(GetPath());
            Current = JsonUtility.FromJson<Settings>(text);
            return true;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }
}