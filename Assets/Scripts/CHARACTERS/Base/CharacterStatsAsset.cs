using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Blank Stats", menuName = "Stats")]
public class CharacterStatsAsset : ScriptableObject
{
    public string _Name;
    public int _Health;
    public int _Mana;
    public int _Attack;
    public int _Defense;
}
