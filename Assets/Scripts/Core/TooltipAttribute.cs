using UnityEngine;

public class TooltipAttribute : PropertyAttribute
{
    public readonly string Text;

    public TooltipAttribute(string text)
    {
        Text = text;
    }
}