﻿using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu(" GrandMythos/UI/UISetColorWorkaround")]
public class UISetColorWorkaround : MonoBehaviour
{
    [Required]
    public Graphic Image;

    public void SetRGBFromHex(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out var color))
            Image.color = new Color(color.r, color.g, color.b, Image.color.a);
    }

    public void SetRGBAFromHex(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out var color))
            Image.color = color;
    }
}