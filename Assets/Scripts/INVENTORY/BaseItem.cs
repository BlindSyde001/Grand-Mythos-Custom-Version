using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseItem : ScriptableObject
{
    [SerializeField]
    internal string _ItemName;
    [SerializeField]
    internal int _ItemAmount;
}
