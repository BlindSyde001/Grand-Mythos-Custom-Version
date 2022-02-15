using System.Collections;
using System.Collections.Generic;
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
    internal int _ItemAmount;
    [BoxGroup("BASIC DATA")]
    [SerializeField]
    [TextArea]
    internal string _ItemDescription;
}
