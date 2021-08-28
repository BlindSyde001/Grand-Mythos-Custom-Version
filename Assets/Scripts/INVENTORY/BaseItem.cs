using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BaseItem : ScriptableObject
{
    [BoxGroup("BASIC DATA")]
    [SerializeField]
    internal int _ItemID;
    [BoxGroup("BASIC DATA")]
    [SerializeField]
    internal string _ItemName;
    [BoxGroup("BASIC DATA")]
    [SerializeField]
    internal int _ItemAmount;
}
