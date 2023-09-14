using System;
using Sirenix.OdinInspector;

[Serializable, InlineProperty]
public struct RangeInt
{
    [HorizontalGroup]
    [ValidateInput(nameof(ValidateMin), "Min must be equal or smaller than Max")]
    public int Min;
    [HorizontalGroup]
    [ValidateInput(nameof(ValidateMax), "Max must be equal or greater than Min")]
    public int Max;

    public RangeInt(int min, int max)
    {
        Min = min;
        Max = max;
    }

    bool ValidateMax(int val, ref string errorMessage)
    {
        if (val >= Min)
            return true;

        return false;
    }

    bool ValidateMin(int val, ref string errorMessage)
    {
        if (val <= Max)
            return true;

        return false;
    }
}