﻿using System;
using UnityEngine;

namespace YNode
{
    /// <summary> Specify a color for this node type </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeTintAttribute : Attribute
    {
        public Color color;

        /// <summary> Specify a color for this node type </summary>
        /// <param name="r"> Red [0.0f .. 1.0f] </param>
        /// <param name="g"> Green [0.0f .. 1.0f] </param>
        /// <param name="b"> Blue [0.0f .. 1.0f] </param>
        public NodeTintAttribute(float r, float g, float b)
        {
            color = new(r, g, b);
        }

        /// <summary> Specify a color for this node type </summary>
        /// <param name="hex"> HEX color value </param>
        public NodeTintAttribute(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out color);
        }

        /// <summary> Specify a color for this node type </summary>
        /// <param name="r"> Red [0 .. 255] </param>
        /// <param name="g"> Green [0 .. 255] </param>
        /// <param name="b"> Blue [0 .. 255] </param>
        public NodeTintAttribute(byte r, byte g, byte b)
        {
            color = new Color32(r, g, b, byte.MaxValue);
        }
    }
}