using System;
using Sirenix.OdinInspector;

namespace Interactables.Conditions
{
    [Serializable]
    public class QuestStepState : ICondition
    {
        [Required, HideLabel, HorizontalGroup, SuffixLabel(" is ")]
        public QuestStep Step;
        [HideLabel, HorizontalGroup]
        public State Check;

        public bool Evaluate()
        {
            bool targetState = Check == State.Done;
            return GameManager.Instance.CompletedSteps.Contains(Step) == targetState;
        }

        public bool IsValid(out string error)
        {
            if (Step == null)
            {
                error = $"{nameof(Step)} is null";
                return false;
            }

            error = "";
            return true;
        }

        public enum State
        {
            Done,
            InProgress,
        }
    }
}