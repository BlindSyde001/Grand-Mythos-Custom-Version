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
            if (Check == State.InProgress)
            {
                int index = Array.IndexOf(Step.Quest.Steps, Step);
                return Step.Completed == false && (index == 0 || Step.Quest.Steps[index - 1].Completed);
            }

            bool targetState = Check == State.Completed;
            return Step.Completed == targetState;
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
            Completed,
            [Tooltip("This step has not been completed yet, but the previous one is")]
            InProgress,
            [Tooltip("This step has not been completed yet, a step can be both 'InProgress' and 'Incomplete'")]
            Incomplete,
        }
    }
}