using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SaveData
{
    private static SaveData _current;
    public static SaveData current
    {
        get
        {
            if (_current == null)
            {
                _current = new SaveData();
            }
            return _current;
        }
    }

    public string savedScene;
    public Vector3 overworldPos;
    public Quaternion overworldRot;

    public List<int> lineupSave = new List<int>();
    public List<SerializableHero> heroSaveData = new List<SerializableHero>();
}

[System.Serializable]
public class SerializableHero
{
    public int totalExperienceSave;
    public string sprite;

    public int weaponIDSave;
    public int armourIDSave;
    public int accessoryOneIDSave;
    public int accessoryTwoIDSave;

    public string weaponSave;
    public string armourSave;
    public bool accessoryOneSave;
    public bool accessoryTwoSave;
}