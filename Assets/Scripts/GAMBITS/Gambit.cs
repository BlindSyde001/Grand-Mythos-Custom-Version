using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
[InlineEditor]
public class Gambit : ScriptableObject
{
    public bool isTurnedOn;                   // Is this Gambit activated?

    public HeroExtension _Hero;               // Who is performing Action
    public CharacterCircuit _Target;          // Who is target of Action

    public Action _Action;                    // What is to be done
    public Condition _Condition;              // What needs to be met to perform Action

    public bool ConditionIsMet;               // Is condition met?

    public CharacterType RetrieveTargetType()
    {
        return _Condition.targetType;        // Either CHARACTER or ENEMY
    }
    public void CallCheck()
    {
       ConditionIsMet = _Condition.ConditionCheck(_Target); // Runs the condition, returns true/false
    }
}