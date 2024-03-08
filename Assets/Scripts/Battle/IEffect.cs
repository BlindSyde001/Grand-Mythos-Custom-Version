using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;

[InlineProperty]
public interface IEffect
{
    IEnumerable Apply(BattleCharacterController[] targets, EvaluationContext context);
    /// <summary> Describes the effect in a human-readable format </summary>
    string UIDisplayText { get; }
}

public static class EffectsExtension
{
    public static string UIDisplayText(this IEffect[] effects)
    {
        if (effects.Length == 0)
            return "";
        if (effects.Length == 1)
            return effects[0]?.UIDisplayText;
        return $"({string.Join(") then (", effects.Select(x => x?.UIDisplayText))})";
    }
}