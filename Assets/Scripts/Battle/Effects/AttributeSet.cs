using System;
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

        public void Apply(BattleCharacterController[] targets, EvaluationContext context)
        {
            foreach (var target in targets)
            {
                int val = Value + UnityEngine.Random.Range(-Variance, Variance+1);
                target.Profile.SetAttribute(Attribute, val);
            }
        }

        public string UIDisplayText => $"set {Attribute} to {Value}";
    }
}