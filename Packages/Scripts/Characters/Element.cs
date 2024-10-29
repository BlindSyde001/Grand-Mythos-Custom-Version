using System;
using UnityEngine;

public enum Element
{
    Neutral,
    Fire,
    Ice,
    Lighting,
    Water,
    [InspectorName(null)] Last = Water
}

public static class ElementExtension
{
    public static Color GetAssociatedColor(this Element element) => element switch
    {
        Element.Neutral => Color.gray,
        Element.Fire => Color.red,
        Element.Ice => Color.cyan,
        Element.Lighting => Color.yellow,
        Element.Water => Color.blue,
        _ => throw new ArgumentOutOfRangeException()
    };
}