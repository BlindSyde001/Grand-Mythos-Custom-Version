using System;
using System.Linq;
using QTE;
using Sirenix.OdinInspector;
using UnityEngine;

[InlineProperty]
public interface IEffect
{
    void Apply(BattleCharacterController[] targets, QTEResult result, EvaluationContext context);
    /// <summary> Describes the effect in a human-readable format </summary>
    string UIDisplayText { get; }

    Color32 UIColor
    {
        get
        {
            var hash = GetHashCode();
            Color32 color;
            unsafe
            {
                color = *(Color32*)&hash;
            }

            int max = 0;
            for (int i = 0; i < 3; i++)
                max = Math.Max(color[i], max);
            for (int i = 0; i < 3; i++)
                color[i] += (byte)(255 - max);
            color.a = 255;
            return color;
        }
    }
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