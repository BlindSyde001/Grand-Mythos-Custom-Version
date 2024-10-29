#nullable enable

using System;
using UnityEngine;

namespace Nodalog
{
    [Serializable]
    public class LineData
    {
        [TextArea(4, 20)] public string RawString = "";
    }
}