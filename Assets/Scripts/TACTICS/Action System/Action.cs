using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public enum ActionTargetType { SINGLE, MULTI};
public enum ActionEffect { DAMAGE, HEAL, OTHER};
public enum ActionType { SKILL, ITEM};
public enum ActionElement { FIRE, ICE, LIGHTNING, WATER, NONE}

[CreateAssetMenu(fileName = "Blank Action", menuName = "Actions")]
public class Action : ScriptableObject
{
    [BoxGroup("Properties")]
    public ActionTargetType ActionTargetType;
    [BoxGroup("Properties")]
    public ActionType ActionType;
    [BoxGroup("Properties")]
    public ActionEffect ActionEffect;
    [BoxGroup("Properties")]
    public ActionElement ActionElement;
    [BoxGroup("Properties")]
    public bool isMagical;
    [BoxGroup("Properties")]
    public bool isFlatAmount;

    [BoxGroup("Basic Info")]
    public string Name;
    [BoxGroup("Basic Info")]
    public int _ActionID;
    [BoxGroup("Basic Info")]
    public string AnimationName;
    [BoxGroup("Basic Info")]
    public float AnimationTiming;
    [BoxGroup("Basic Info")]
    [TextArea]
    public string Description;

    [BoxGroup("Modifiers")]
    public int _Cost;
    [BoxGroup("Modifiers")]
    public int critChance;
    [BoxGroup("Modifiers")]
    public float PowerModifier;  // First power modifier e.g initial spell power, attack damage modifier
    [BoxGroup("Modifiers")]
    public float PowerModifier2; // Second power modifier e.g top end range for damage variation (attack damage is between 1 - 1.5)
    [BoxGroup("Modifiers")]
    public float TimingModifier; // Use in countdowns, e.g Doom spell, Damage over time
    public List<ActionBehaviour> Behaviours;
}
