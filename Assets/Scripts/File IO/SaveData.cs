using System.Collections.Generic;
using UnityEngine;

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
    public guid weaponIDSave;
    public guid armourIDSave;
    public guid accessoryOneIDSave;
    public guid accessoryTwoIDSave;

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
public class SerializableInventory
{
    public List<guid> ConsumablesIdData = new();
    public List<int> ConsumablesAmountData = new();

    public List<string> EquipmentNameData = new();
    public List<guid> EquipmentIdData = new();
    public List<int> EquipmentAmountData = new();

    public List<guid> LootIdData = new();
    public List<int> LootAmountData = new();

    public List<guid> KeyItemsIdData = new();
    public List<int> ConditionsIdData = new();
    public int CreditsAmountData = new();
}

[System.Serializable]
public class SerializableTacticController       // Tactics consist of {Toggle, Condition, Actions & their Conditions}
{
    public List<bool> tacticToggleList = new() 
    {
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false
    };
    public List<string> tacticCndList = new() 
    {
        "",
        "",
        "",
        "",
        "",
        "",
        "",
        "",
        "",
        ""
    };
    public List<SerializableTacticActions> tacticActionCapsulesList = new()
    {
        //new SerializableTacticActions(),
        //new SerializableTacticActions(),
        //new SerializableTacticActions(),
        //new SerializableTacticActions(),
        //new SerializableTacticActions(),
        //new SerializableTacticActions(),
        //new SerializableTacticActions(),
        //new SerializableTacticActions(),
        //new SerializableTacticActions(),
        //new SerializableTacticActions(),
    };
}

[System.Serializable]
public class SerializableTacticActions
{
    public List<string> tacticActions = new()
    {
        "",
        "",
        "",
        ""
    };
    public List<string> tacticActionTypes = new()
    {
        "",
        "",
        "",
        ""
    };
}