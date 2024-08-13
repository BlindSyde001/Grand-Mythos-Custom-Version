using System;
using System.Linq;
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

    [SerializeField] float _enmityGenerationTarget = 4f;
    [FormerlySerializedAs("_enmityGenerationHostiles")] [SerializeField] float _enmityGenerationNonTarget = 1f;

    [Tooltip("When this action is used, skills attached to the unit's weapon can proc")]
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
    public Condition TargetConstraint;

    [BoxGroup(nameof(PreconditionToUse), LabelText = @"@""Precondition To Use:   "" + this.PreconditionToUse?.UIDisplayText")]
    [DetailedInfoBox(PreconditionInfoTextShort, PreconditionInfoText)]
    [HideLabel]
    [SerializeReference]
    public Condition PreconditionToUse;

    Condition IAction.TargetFilter => TargetConstraint;
    Condition IAction.Precondition => PreconditionToUse;

    public void Perform(BattleCharacterController[] targets, EvaluationContext context)
    {
        foreach (var effect in Effects)
        {
            effect.Apply(targets, context);
        }

        if (!ProcAttachedSkills
            || context.Profile is not HeroExtension hero
            || hero._Weapon == null
            || hero._Weapon.AttachedSkill == null
            || context.Random.NextFloat(0, 100) > hero._Weapon.SkillProcChance) 
            return;

        var attachedSkill = hero._Weapon.AttachedSkill;
        var attachmentTargets = new TargetCollection(targets.ToList());
        
        if (BattleStateMachine.TryGetInstance(out var battle))
        {
            try
            {
                if (attachedSkill.PreconditionToUse != null)
                {
                    var allTargetsCopy = new TargetCollection(battle.Units);
                    attachedSkill.PreconditionToUse.Filter(ref allTargetsCopy, context.Controller.Context);

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

    public string UIDisplayText => Effects.UIDisplayText();

    string IAction.Name => name;
    public string Description => string.IsNullOrWhiteSpace(_description) ? $"No Description - falling back to auto generated; {UIDisplayText}" : _description;
    public float EnmityGenerationTarget => _enmityGenerationTarget;
    public float EnmityGenerationNonTarget => _enmityGenerationNonTarget;


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