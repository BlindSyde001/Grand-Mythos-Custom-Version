using UnityEngine;
using Sirenix.OdinInspector;

public class Equipment : TradeableItem
{
    [BoxGroup("OFFENSE")]
    [SerializeField]
    internal int _EquipAttack;
    [BoxGroup("OFFENSE")]
    [SerializeField]
    internal int _EquipMagAttack;

    [BoxGroup("DEFENSE")]
    [SerializeField]
    internal int _EquipHP;
    [BoxGroup("DEFENSE")]
    [SerializeField]
    internal int _EquipMP;
    [BoxGroup("DEFENSE")]
    [SerializeField]
    internal int _EquipDefense;
    [BoxGroup("DEFENSE")]
    [SerializeField]
    internal int _EquipMagDefense;
}
