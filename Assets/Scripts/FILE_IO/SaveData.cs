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

    // Current Scene
    public string scene;
    // Overworld Position
    public Vector3 overworldPos;
    // Overworld Rotation
    public Quaternion overworldRot;

    public List<HeroExtension> allPartyMembersSave;
    public List<HeroExtension> partyLineupSave;

    public List<SerializableHero> heroSaveData = new List<SerializableHero>();
    // Available items in Inventory => List<scriptableobject>
    // Inventory Orgaisation order => List<scriptableobject>
}

[System.Serializable]
public class SerializableHero
{
    public int totalExperienceSave;

    public int weaponIDSave;
    public int armourIDSave;
    public int accessoryOneIDSave;
    public int accessoryTwoIDSave;

    public Weapon weaponSave;
    public Armour armourSave;
    public Accessory accessoryOneSave;
    public Accessory accessoryTwoSave;
}