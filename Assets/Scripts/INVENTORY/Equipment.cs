using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EquipmentType { Weapon, Armour, Accessory }
[CreateAssetMenu(fileName = "New Equipment", menuName = "Equipment")]
public class Equipment : BaseItem
{
    [Header("OFFENSE")]

    [SerializeField]
    internal int _EquipAttack;
    [SerializeField]
    internal int _EquipMagAttack;

    [Header("DEFENSE")]

    [SerializeField]
    internal int _EquipDefense;
    [SerializeField]
    internal int _EquipMagDefense;
}
