using System.Collections.Generic;
using UnityEngine;

public class EnemyExtension : CharacterTemplate
{

    [SerializeField]
    internal BattleEnemyModelController myBattleEnemyController;

    [SerializeField]
    internal int experiencePool;                // How much EXP the enemy Gives
    [SerializeField]
    internal int creditPool;                    // How many Credits the enemy Gives

    [SerializeField]
    internal List<ItemCapsule> DropItems;       // Loot that the enemy drops
    [SerializeField]
    internal List<float> DropRate;

    public override void AssignStats()
    {
        charName = _CSA._Name;
        _MaxHP = _CSA._BaseHP;
        _MaxMP = _CSA._BaseMP;
        _Attack = _CSA._BaseAttack;
        _MagAttack = _CSA._BaseMagAttack;
        _Defense = _CSA._BaseDefense;
        _MagDefense = _CSA._BaseMagDefense;

        ActionRechargeSpeed = _CSA._BaseSpeed;
        _CurrentHP = _MaxHP;
        _CurrentMP = _MaxMP;
    }
}
