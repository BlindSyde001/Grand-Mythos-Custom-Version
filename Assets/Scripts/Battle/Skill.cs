using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu] 
public class Skill : IdentifiableScriptableObject, IAction
{
    const string TargetInfoTextShort = "The state the TARGET of this action MUST ABSOLUTELY BE IN to be able to use this action ON THEM.";
    const string TargetInfoText = "The state the TARGET of this action MUST ABSOLUTELY BE IN to be able to use this action ON THEM.\n" +
                                  "Do not check for missing health when this action heals, that's for Tactics to decide";

    const string PreconditionInfoTextShort = "What MUST ABSOLUTELY be true to be able to use this action.";
    const string PreconditionInfoText = "What MUST ABSOLUTELY be true to be able to use this action.\n" +
                                        "This is more for skills that should NEVER be used in a specific context. Eg: skills requiring mana to be used when self doesn't have enough mana";

    [Space]
    public uint ATBCost = 1;

    [Space]
    [ListDrawerSettings(ShowFoldout = false)]
    [LabelText(@"@""Effects:   "" + this.UIDisplayText")]
    [SerializeReference]
    public IEffect[] Effects = Array.Empty<IEffect>();

    [BoxGroup(nameof(TargetConstraint), LabelText = @"@""Target Constraint:   "" + this.TargetConstraint?.UIDisplayText")]
    [DetailedInfoBox(TargetInfoTextShort, TargetInfoText)]
    [HideLabel]
    [SerializeReference]
    public Condition TargetConstraint;

    [BoxGroup(nameof(PreconditionToUse), LabelText = @"@""Precondition To Use:   "" + this.PreconditionToUse?.UIDisplayText")]
    [DetailedInfoBox(PreconditionInfoTextShort, PreconditionInfoText)]
    [HideLabel]
    [SerializeReference]
    public Condition PreconditionToUse;

    uint IAction.ActionCost => ATBCost;
    Condition IAction.TargetFilter => TargetConstraint;
    Condition IAction.Precondition => PreconditionToUse;

    public IEnumerable Perform(BattleCharacterController[] targets, EvaluationContext context)
    {
        foreach (var effect in Effects)
        {
            foreach (var yield in effect.Apply(targets, context))
                yield return yield;
        }
    }

    public string UIDisplayText => Effects.UIDisplayText();

    string IAction.Name => name;
}