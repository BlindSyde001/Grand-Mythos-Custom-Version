using System;
using UnityEngine;

public class ConstrainedTypeAttribute : PropertyAttribute
{
    public Type Type;
    public ConstrainedTypeAttribute(Type interfaceType)
    {
        Type = interfaceType;
    }
}