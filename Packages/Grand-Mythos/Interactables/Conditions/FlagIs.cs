using Nodalog;
using Sirenix.OdinInspector;

namespace Interactables.Conditions
{
    public class FlagIs : ICondition
    {
        [HorizontalGroup, HideLabel]
        public required Flag Flag;

        [HorizontalGroup, LabelText(" == "), SuffixLabel("?")]
        public bool State = true;

        public bool Evaluate() => Flag.State == State;

        public bool IsValid(out string error)
        {
            if (Flag == null!)
            {
                error = $"{nameof(Flag)} is null";
                return false;
            }

            error = "";
            return true;
        }
    }
}