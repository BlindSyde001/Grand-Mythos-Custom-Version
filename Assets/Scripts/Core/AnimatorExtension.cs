using UnityEngine;

public static class AnimatorExtension
{
    public static void Play(this Animator animator, AnimationState animationState, float normalizedTime = 0f)
    {
        animator.Play(animationState.StateName, animationState.Layer, normalizedTime);
    }

    public static bool IsPlaying(this Animator animator, AnimationState animationState)
    {
        return animator.GetCurrentAnimatorStateInfo(animationState.Layer).IsName(animationState.StateName);
    }
}