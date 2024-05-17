#nullable enable

using System;
using UnityEngine;

namespace Nodalog
{
    [Serializable]
    public class LineData
    {
        [Multiline(lines:10)] public string RawString = "";
    }
}