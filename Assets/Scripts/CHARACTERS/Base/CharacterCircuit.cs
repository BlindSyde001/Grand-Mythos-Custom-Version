using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCircuit : MonoBehaviour
{
    [SerializeField]
    internal CharacterStatsAsset _CSA;

    #region STATS
    internal int hp;
    internal int attack;
    #endregion

    public float _ActionChargeAmount;
    [SerializeField]
    internal float _ActionRecharge;

    private void Awake()
    {
        if (hp != 0)
            hp = _CSA._Health;
        if (attack != 0)
            attack = _CSA._Attack;
    }
    private void Update()
    {
        _ActionChargeAmount += _ActionRecharge * Time.deltaTime;
        _ActionChargeAmount = Mathf.Clamp(_ActionChargeAmount, 0, 100);
        if(_ActionChargeAmount >= 100)
        {
            Attack();
            _ActionChargeAmount = 0;
        }
    }
    internal virtual void Attack()
    {

    }
    internal virtual void Magic()
    {

    }
    internal virtual void Item()
    {

    }
}
