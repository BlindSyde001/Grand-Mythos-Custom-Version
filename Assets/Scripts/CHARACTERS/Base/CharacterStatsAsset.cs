using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public enum CharacterType { CHARACTER, ENEMY};
[CreateAssetMenu(fileName = "Blank Stats", menuName = "Stats")]
public class CharacterStatsAsset : ScriptableObject
{
    [BoxGroup("Basic Info")]
    public CharacterType _CharacterType;
    [BoxGroup("Basic Info")]
    public string _Name;
    [BoxGroup("Basic Info")]
    public int _BaseExperience;
    [Space(20)]


    [VerticalGroup("Stats")]
    [LabelWidth(100)]
    public int startingLevel;
    [VerticalGroup("Stats")]
    [LabelWidth(100)]
    [GUIColor(0.5f, 1f, 0.5f)]
    public int _BaseHP;

    [VerticalGroup("Stats")]
    [LabelWidth(100)]
    [GUIColor(0.5f, 0.5f, 0.9f)]
    public int _BaseMP;

    [VerticalGroup("Stats")]
    [LabelWidth(100)]
    [GUIColor(1f, 0.5f, 0.5f)]
    public int _BaseAttack;

    [VerticalGroup("Stats")]
    [LabelWidth(100)]
    [GUIColor(1f, 0.5f, 0.5f)]
    public int _BaseMagAttack;

    [VerticalGroup("Stats")]
    [LabelWidth(100)]
    [GUIColor(0.5f, 0.8f, 0.8f)]
    public int _BaseDefense;
    [VerticalGroup("Stats")]
    [LabelWidth(100)]
    [GUIColor(0.5f, 0.8f, 0.8f)]
    public int _BaseMagDefense;
    [VerticalGroup("Stats")]
    [LabelWidth(100)]
    public int _BaseSpeed;
}
