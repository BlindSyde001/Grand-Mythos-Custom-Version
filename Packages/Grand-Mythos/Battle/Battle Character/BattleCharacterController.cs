using Sirenix.OdinInspector;
using UnityEngine;

[AddComponentMenu(" GrandMythos/BattleCharacterController")]
public class BattleCharacterController : MonoBehaviour
{
    [ReadOnly] public required CharacterTemplate Profile;

    [ValidateInput(nameof(ValidateAnim))]
    public AnimationState IdleAnimation;

    [ValidateInput(nameof(ValidateAnim))]
    public AnimationState DeathAnimation;

    public required Animator Animator;

    private EvaluationContext? _context;

    public EvaluationContext Context => _context ??= new EvaluationContext(Profile);

    void Start()
    {
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

