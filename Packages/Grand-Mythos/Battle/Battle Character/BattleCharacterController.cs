using System;
using System.Linq;
using ActionAnimation;
using QTE;
using Sirenix.OdinInspector;
using UnityEngine;

[AddComponentMenu(" GrandMythos/BattleCharacterController")]
public class BattleCharacterController : MonoBehaviour
{
    [ReadOnly] public CharacterTemplate Profile;

    [ValidateInput(nameof(ValidateAnim))]
    public AnimationState IdleAnimation;

    [ValidateInput(nameof(ValidateAnim))]
    public AnimationState DeathAnimation;

    [Required]
    public Animator Animator;

    [NonSerialized]
    public EvaluationContext Context;

    public bool IsHostileTo(BattleCharacterController character)
    {
        return Profile.Team.Allies.Contains(character.Profile.Team) == false;
    }

    public BattleCharacterController()
    {
        Context = new(this);
    }

    void Start()
    {
        foreach (var clip in Animator.runtimeAnimatorController.animationClips)
        {
            var eventsCopy = clip.events;
            for (var i = 0; i < eventsCopy.Length; i++)
            {
                var clipEvent = eventsCopy[i];
                if (clipEvent.objectReferenceParameter is QTEStart or QTEEnd)
                {
                    clipEvent.functionName = nameof(AnimationEventCallback.HandleQTEEvent);
                    clipEvent.messageOptions = SendMessageOptions.RequireReceiver;
                    if (clipEvent.objectReferenceParameter is QTEStart)
                    {
                        if (eventsCopy.Skip(i).FirstOrDefault(x => x.objectReferenceParameter is QTEEnd) is { } endEvent)
                            clipEvent.floatParameter = endEvent.time - clipEvent.time;
                        else
                            clipEvent.floatParameter = clip.length - clipEvent.time;
                    }

                    eventsCopy[i] = clipEvent;
                }
            }
            clip.events = eventsCopy;
        }

        if (BattleStateMachine.TryGetInstance(out var bts))
            bts.Include(this);
    }

    void Update()
    {
        if (Profile.CurrentHP != 0 && Animator.IsPlaying(DeathAnimation))
            Animator.Play(IdleAnimation);
    }

    void OnDestroy()
    {
        if (BattleStateMachine.TryGetInstance(out var bts))
            bts.Exclude(this);
    }

    void OnDrawGizmosSelected()
    {
        this.AutoAssign(ref Animator);
    }

    void OnValidate()
    {
        DeathAnimation.EditorOnlyValidate(Animator, out _);
    }

    bool ValidateAnim(AnimationState anim, ref string errorMessage) => anim.EditorOnlyValidate(Animator, out errorMessage);
}

