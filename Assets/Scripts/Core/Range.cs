using System;
using Sirenix.OdinInspector;

[Serializable, InlineProperty]
public struct Range
{
    [HorizontalGroup]
    [ValidateInput(nameof(ValidateMin), "Min must be equal or smaller than Max")]
    public float Min;
    [HorizontalGroup]
    [ValidateInput(nameof(ValidateMax), "Max must be equal or greater than Min")]
    public float Max;

    public Range(float min, float max)
    {
        Min = min;
        Max = max;
    }

    bool ValidateMax(float val, ref string errorMessage)
    {
        if (val >= Min)
            return true;

        return false;
    }

    bool ValidateMin(float val, ref string errorMessage)
    {
        if (val <= Max)
            return true;

        return false;
    }
}