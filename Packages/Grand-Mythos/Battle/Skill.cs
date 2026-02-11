using System;
using System.Linq;
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
    
    [SerializeField, TextArea]
    string _description = "";

    public int ManaCost;

    [Range(0f,100f)]
    public float FlowCost;

    public IAction.Delay DelayToNextTurn = IAction.Delay.Base;

    [Tooltip("When this action is used, should the skill attached to the unit's weapon proc")]
    public bool ProcAttachedSkills = false;

    [Space]
    [ListDrawerSettings(ShowFoldout = false, OnBeginListElementGUI = nameof(BeginDrawEffect), OnEndListElementGUI = nameof(EndDrawEffect))]
    [LabelText(@"@""Effects:   "" + this.UIDisplayText")]
    [SerializeReference]
    public IEffect[] Effects = Array.Empty<IEffect>();

    [BoxGroup(nameof(TargetConstraint), LabelText = @"@""Target Constraint:   "" + this.TargetConstraint?.UIDisplayText")]
    [DetailedInfoBox(TargetInfoTextShort, TargetInfoText)]
    [HideLabel]
    [SerializeReference]
    public Condition? TargetConstraint;

    [BoxGroup(nameof(PreconditionToUse), LabelText = @"@""Precondition To Use:   "" + this.PreconditionToUse?.UIDisplayText")]
    [DetailedInfoBox(PreconditionInfoTextShort, PreconditionInfoText)]
    [HideLabel]
    [SerializeReference]
    public Condition? PreconditionToUse;

    public AnimationClip? CameraAnimation;

    Condition? IAction.TargetFilter => TargetConstraint;
    Condition? IAction.Precondition => PreconditionToUse;
    IAction.Delay IAction.DelayToNextTurn => DelayToNextTurn;
    AnimationClip? IAction.CameraAnimation => CameraAnimation;
    int IAction.ManaCost => ManaCost;
    float IAction.FlowCost => FlowCost;

    public string UIDisplayText => Effects.UIDisplayText();
    string IAction.Name => name;
    public string Description => string.IsNullOrWhiteSpace(_description) ? $"No Description - falling back to auto generated; {UIDisplayText}" : _description;

    public void Perform(CharacterTemplate[] targets, EvaluationContext context)
    {
        context.Profile.CurrentMP -= ManaCost;
        context.Profile.CurrentFlow -= FlowCost;
        
        foreach (var effect in Effects)
        {
            effect.Apply(targets, context);
        }

        if (!ProcAttachedSkills || context.Profile is not HeroExtension hero) 
            return;

        foreach (var (attachedSkill, chance) in hero.AttachedSkills)
        {
            if (context.Random.NextFloat(0, 100) > chance)
                continue;
            
            var attachmentTargets = new TargetCollection(targets.ToList());
        
            if (BattleStateMachine.TryGetInstance(out var battle))
            {
                try
                {
                    if (attachedSkill.PreconditionToUse != null)
                    {
                        var allTargetsCopy = new TargetCollection(battle.Units.Select(x => x.Profile).ToList());
                        attachedSkill.PreconditionToUse.Filter(ref allTargetsCopy, context);

                        if (allTargetsCopy.IsEmpty)
                        {
                            return;
                        }
                    }
                }
                catch(Exception e) when (attachedSkill is UnityEngine.Object o)
                {
                    Debug.LogException(e, o);
                }
            }
            else
            {
                Debug.LogWarning($"Could not retrieve the {nameof(BattleStateMachine)} to run the precondition for {nameof(Weapon.AttachedSkill)}");
            }
        
            attachedSkill.TargetConstraint?.Filter(ref attachmentTargets, context);
        
            var attachmentFilteredTargets = attachmentTargets.ToArray();
            if (attachmentFilteredTargets.Length == 0)
                return;

            foreach (var effect in attachedSkill.Effects)
            {
                effect.Apply(attachmentFilteredTargets, context);
            }

            attachedSkill.TargetConstraint?.NotifyUsedCondition(attachmentTargets, context);
            attachedSkill.PreconditionToUse?.NotifyUsedCondition(attachmentTargets, context);
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