using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu] 
public class Skill : IdentifiableScriptableObject, IAction
{
    const string TargetInfoTextShort = "The state the TARGET of this action MUST ABSOLUTELY BE IN to be able to use this action ON THEM.";
    const string TargetInfoText = "The state the TARGET of this action MUST ABSOLUTELY BE IN to be able to use this action ON THEM.\n" +
                                  "Do not check for missing health when this action heals, that's for Tactics to decide";

    const string PreconditionInfoTextShort = "What MUST ABSOLUTELY be true to be able to use this action.";
    const string PreconditionInfoText = "What MUST ABSOLUTELY be true to be able to use this action.\n" +
                                        "This is more for skills that should NEVER be used in a specific context. Eg: skills requiring mana to be used when self doesn't have enough mana";

    [FormerlySerializedAs("Description"), SerializeField, TextArea]
    string _description = "";

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

    public void Perform(BattleCharacterController[] targets, EvaluationContext context)
    {
        foreach (var effect in Effects)
        {
            effect.Apply(targets, context);
        }
    }

    public string UIDisplayText => Effects.UIDisplayText();

    string IAction.Name => name;
    public string Description => string.IsNullOrWhiteSpace(_description) ? $"No Description - falling back to auto generated; {UIDisplayText}" : _description;
}