using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestIO : MonoBehaviour
{
    public void ClickSave()
    {
        GameManager._instance._LastKnownScene = SceneManager.GetActiveScene().name;
        GameManager._instance._LastKnownPosition = FindObjectOfType<OverworldPlayerCircuit>().transform.position;
        GameManager._instance._LastKnownRotation = FindObjectOfType<OverworldPlayerCircuit>().transform.rotation;

        SaveData.current.scene = GameManager._instance._LastKnownScene;

        SaveData.current.overworldPos = GameManager._instance._LastKnownPosition;
        SaveData.current.overworldRot = GameManager._instance._LastKnownRotation;

        SaveData.current.allPartyMembersSave = GameManager._instance._AllPartyMembers;
        SaveData.current.partyLineupSave = GameManager._instance._PartyLineup;

        foreach(HeroExtension hero in GameManager._instance._AllPartyMembers)
        {
            SerializableHero heroSave = new SerializableHero
            {
                totalExperienceSave = hero._TotalExperience,

                weaponIDSave = hero._Weapon._ItemID,
                armourIDSave = hero._Armour._ItemID,
                accessoryOneIDSave = hero._AccessoryOne._ItemID,
                accessoryTwoIDSave = hero._AccessoryTwo._ItemID,

                weaponSave = hero._Weapon,
                armourSave = hero._Armour,
                accessoryOneSave = hero._AccessoryOne,
                accessoryTwoSave = hero._AccessoryTwo
            };

            SaveData.current.heroSaveData.Add(heroSave);
        }
        SaveManager.SaveToFile(SaveData.current, "testSave");
        Debug.Log("Saved");
    }
    public void ClickLoad(string fileName)
    {
       var SD = SaveManager.LoadFromFile(fileName);

        GameManager._instance._LastKnownScene = SD.scene;
        GameManager._instance._LastKnownPosition = SD.overworldPos;
        GameManager._instance._LastKnownRotation = SD.overworldRot;

        GameManager._instance._AllPartyMembers = SD.allPartyMembersSave;
        GameManager._instance._PartyLineup = SD.partyLineupSave;

        for(int i = 0; i < GameManager._instance._AllPartyMembers.Count; i++)
        {
            GameManager._instance._AllPartyMembers[i]._TotalExperience = SD.heroSaveData[i].totalExperienceSave;
            SaveManager.ExtractWeaponData(SD, i);
            SaveManager.ExtractArmourData(SD, i);
            SaveManager.ExtractAccessoryData(SD, i);
        }
    }
}
