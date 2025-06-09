using System;
using QTE;
using Sirenix.OdinInspector;

namespace Effects
{
    [Serializable]
    public class AttributeSet : IEffect
    {
        [HorizontalGroup, HideLabel, SuffixLabel("=")]
        public Attribute Attribute = Attribute.Health;
        [HorizontalGroup, HideLabel]
        public int Value = 100;
        public int Variance = 0;

        public void Apply(BattleCharacterController[] targets, QTEResult result, EvaluationContext context)
        {
            foreach (var target in targets)
            {
                int val = Value + UnityEngine.Random.Range(-Variance, Variance+1);
                if (result is QTEResult.Correct or QTEResult.Success)
                    target.Profile.SetAttribute(Attribute, val);
            }
        }

        public string UIDisplayText => $"Set {Attribute} to {Value}";
    }
}