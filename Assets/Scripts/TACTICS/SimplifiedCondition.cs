namespace Conditions
{
    /// <summary>
    /// A <see cref="Condition"/> with a simplified way to filter targets.
    /// </summary>
    public abstract class SimplifiedCondition : Condition
    {
        protected override void FilterInner(ref TargetCollection targets, EvaluationContext context)
        {
            for (int i = -1; targets.TryGetNext(ref i, out var target); )
            {
                if (Filter(target, context) == false)
                    targets.RemoveAt(i);
            }
        }

        /// <summary>
        /// Returns whether this <paramref name="target"/> matches this condition
        /// </summary>
        protected abstract bool Filter(BattleCharacterController target, EvaluationContext context);
    }
}