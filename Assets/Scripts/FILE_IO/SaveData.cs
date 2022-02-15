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

    public int playTimeData;

    public List<int> lineupSave = new();
    public List<SerializableHero> heroSaveData = new();
    public List<SerializableTacticController> heroTacticData = new();
    public SerializableInventory inventorySaveData = new();
}

[System.Serializable]
public class SerializableHero
{
    public int totalExperienceSave;
    public string sprite;
    #region Player Loadout Data
    public int weaponIDSave;
    public int armourIDSave;
    public int accessoryOneIDSave;
    public int accessoryTwoIDSave;

    public string weaponSave;
    public string armourSave;
    public bool accessoryOneSave;
    public bool accessoryTwoSave;
    #endregion
    #region Player Health Data
    public int currentHPData;
    public int currentMPData;
    #endregion
}

[System.Serializable]
public class SerializableTacticController
{
    public List<bool> tacticToggleList = new() { false, false, false, false, false, false, false, false, false, false };
    public List<string> tacticCndList = new() { "", "", "", "", "", "", "", "", "", "" };

    public List<string> tacticActionTypeList = new() { "", "", "", "", "", "", "", "", "", "" };
    public List<string> tacticActionList = new() { "", "", "", "", "", "", "", "", "", "" };
}

[System.Serializable]
public class SerializableInventory
{
    public List<int> ConsumablesIdData = new();
    public List<int> ConsumablesAmountData = new();

    public List<string> EquipmentNameData = new();
    public List<int> EquipmentIdData = new();
    public List<int> EquipmentAmountData = new();

    public List<int> LootIdData = new();
    public List<int> LootAmountData = new();

    public List<int> KeyItemsIdData = new();
    public List<int> ConditionsIdData = new();
    public int CreditsAmountData = new();
}