using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class HS_Officer : HeroExtension
{
    internal override int BaseHP { get => (int)(_CSA._BaseHP * (1 + _GrowthRateAverage) * _Level); }
    internal override int BaseMP { get => (int)(_CSA._BaseMP * (1 + _GrowthRateAverage) * _Level); }
    internal override int BaseAttack { get => (int)(_CSA._BaseAttack * (1 + _GrowthRateAverage) * _Level); }
    internal override int BaseMagAttack { get => (int)(_CSA._BaseMagAttack * (1 + _GrowthRateAverage) * _Level); }
    internal override int BaseDefense { get => (int)(_CSA._BaseDefense * (1 + _GrowthRateAverage) * _Level); }
    internal override int BaseMagDefense { get => (int)(_CSA._BaseMagDefense * (1 + _GrowthRateAverage) * _Level); }
    internal override int BaseSpeed { get => _CSA._BaseSpeed; }

    protected override void Awake()
    {
        base.Awake();
        AssignStats();
    }
    public override void AssignStats()
    {
        _MaxHP = BaseHP + equipHP;
        _MaxMP = BaseMP + equipMP;
        _Attack = BaseAttack + equipAttack;
        _MagAttack = BaseMagAttack + equipMagAttack;
        _Defense = BaseDefense  + equipDefense;
        _MagDefense = BaseMagDefense + equipMagDefense;
        _Speed = BaseSpeed + equipSpeed;

        ActionRechargeSpeed = _Speed;
        _CurrentHP = _MaxHP;
        _CurrentMP = _MaxMP;
    }
}
