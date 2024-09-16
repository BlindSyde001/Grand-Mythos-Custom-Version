using System;
using System.Diagnostics.CodeAnalysis;
using Battle;
using Conditions;
using Effects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Consumables")]
public class Consumable : TradeableItem, IAction
{
    const string TargetInfoTextShort = "The state the TARGET of this consumable MUST ABSOLUTELY BE IN to be able to use this consumable ON THEM.";
    const string TargetInfoText = "The state the TARGET of this consumable MUST ABSOLUTELY BE IN to be able to use this consumable ON THEM.\n" +
                                  "Do not check for missing health when this consumable heals, that's for Tactics to decide";

    const string PreconditionInfoTextShort = "What MUST ABSOLUTELY be true to be able to use this consumable.";
    const string PreconditionInfoText = "What MUST ABSOLUTELY be true to be able to use this consumable.\n" +
                                        "Do not check if the person has one instance of this consumable, this is always checked.\n" +
                                        "This is more for items that should NEVER be used in a specific context. Eg: Items requiring mana to be used when self doesn't have enough mana";

    public int ManaCost = 0;

    [Range(0f,100f)]
    public float FlowCost;
    
    [Tooltip("How long the character has to charge the action before it can be executed")]
    public float ChargeDuration = 0f;

    [SerializeReference]
    [MaybeNull]
    public Channeling Channeling;

    [Space]
    [ListDrawerSettings(ShowFoldout = false, OnBeginListElementGUI = nameof(BeginDrawEffect), OnEndListElementGUI = nameof(EndDrawEffect))]
    [LabelText(@"@""Effects:   "" + this.UIDisplayText")]
    [SerializeReference]
    public IEffect[] Effects;

    [BoxGroup(nameof(TargetConstraint), LabelText = @"@""Target Constraint:   "" + this.TargetConstraint?.UIDisplayText")]
    [DetailedInfoBox(TargetInfoTextShort, TargetInfoText)]
    [HideLabel]
    [SerializeReference]
    public Condition TargetConstraint;

    [BoxGroup(nameof(InnerPrecondition), LabelText = @"@""Precondition To Use:   "" + this.InnerPrecondition?.UIDisplayText")]
    [DetailedInfoBox(PreconditionInfoTextShort, PreconditionInfoText)]
    [HideLabel]
    [SerializeReference]
    public Condition InnerPrecondition;

    public AnimationClip CameraAnimation;

    [NonSerialized] And _basePrecondition;
    [NonSerialized] And _fullPrecondition;

    float IAction.ChargeDuration => ChargeDuration;
    Channeling IAction.Channeling => Channeling;
    Condition IAction.TargetFilter => TargetConstraint;
    AnimationClip IAction.CameraAnimation => CameraAnimation;
    int IAction.ManaCost => ManaCost;
    float IAction.FlowCost => FlowCost;

    /// <summary>
    /// This one contains both the has item test and the additional precondition specific to this consumable
    /// </summary>
    public Condition Precondition
    {
        get
        {
            _basePrecondition ??= new IsSelf() & new ItemCarried { Item = this, AtLeastThisAmount = 1 };
            if (InnerPrecondition == null)
                return _basePrecondition;
            _fullPrecondition ??= new();
            _fullPrecondition.Left = _basePrecondition;
            _fullPrecondition.Right = InnerPrecondition;
            return _fullPrecondition;
        }
    }

    public string UIDisplayText => Effects.UIDisplayText();
    string IAction.Name => name;
    string IAction.Description => string.IsNullOrWhiteSpace(Description) ? $"No Description - falling back to auto generated; {UIDisplayText}" : Description;

    public Consumable()
    {
        Effects = new IEffect[]
        {
            new ApplyOnCaster
            {
                Effects = new IEffect[]
                {
                    new RemoveItem { Item = this, Amount = 1 }
                }
            }
        };
    }

    public void Perform(BattleCharacterController[] targets, EvaluationContext context)
    {
        context.Profile.CurrentMP -= ManaCost;

        foreach (var effect in Effects)
        {
            effect.Apply(targets, context);
        }
    }

    void BeginDrawEffect(int index)
    {
        #if UNITY_EDITOR
        var effect = Effects[index];
        var text = effect?.UIDisplayText ?? "null";
        var color = effect?.UIColor ?? Color.black;
        var prevContent = GUI.backgroundColor;
        GUI.backgroundColor = color;
        Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(text, true);
        GUI.backgroundColor = prevContent;
        #endif
    }

    void EndDrawEffect(int index)
    {
        #if UNITY_EDITOR
        Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
        #endif
    }
}