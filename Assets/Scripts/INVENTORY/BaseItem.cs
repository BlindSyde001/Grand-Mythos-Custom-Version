using UnityEngine;
using Sirenix.OdinInspector;

public enum ItemType { CONSUMABLE, WEAPON, ARMOUR, ACCESSORY, KEYITEM, LOOT};
public class BaseItem : ScriptableObject
{
    [BoxGroup("BASIC DATA")]
    [SerializeField]
    internal ItemType _ItemType;
    [BoxGroup("BASIC DATA")]
    [SerializeField]
    internal int _ItemID;
    [BoxGroup("BASIC DATA")]
    [SerializeField]
    internal string _ItemName;
    [BoxGroup("BASIC DATA")]
    [SerializeField]
    [TextArea]
    internal string _ItemDescription;
}

[System.Serializable]
public class ItemCapsule
{
    public BaseItem thisItem;
    public int ItemID { get { return thisItem._ItemID; } }
    public int ItemAmount;
}