using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public enum ActionTargetType { SINGLE, MULTI};
public enum ActionType { ABILITY, SPELLS};
public enum ActionEffect { DAMAGE, HEAL, OTHER};
public enum ActionElement { FIRE, ICE, LIGHTNING, WATER, NONE}

[CreateAssetMenu(fileName = "Blank Action", menuName = "Actions")]
public class Action : ScriptableObject
{
    [BoxGroup("Properties")]
    public ActionTargetType _ActionTargetType;
    [BoxGroup("Properties")]
    public ActionType _ActionType;
    [BoxGroup("Properties")]
    public ActionEffect _ActionEffect;
    [BoxGroup("Properties")]
    public ActionElement _ActionElement;
    [BoxGroup("Properties")]
    public bool isMagical;

    [BoxGroup("Basic Info")]
    public string _Name;
    [BoxGroup("Basic Info")]
    public int _ActionID;
    [BoxGroup("Basic Info")]
    [TextArea]
    public string _Description;
    [BoxGroup("Basic Info")]
    public int _Cost;

    [BoxGroup("Modifiers")]
    public int critChance;
    [BoxGroup("Modifiers")]
    public float powerModifier;  // First power modifier e.g initial spell power, attack damage modifier
    [BoxGroup("Modifiers")]
    public float powerModifier2; // Second power modifier e.g top end range for damage variation (attack damage is between 1 - 1.5)
    [BoxGroup("Modifiers")]
    public float timingModifier; // Use in countdowns, e.g Doom spell, Damage over time
    public List<ActionBehaviour> _Behaviours;
}
