﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageModal : MonoBehaviour
{
    public TMPro.TMP_Text Header;
    public TMPro.TMP_InputField Message;
    public Button Button;

    static GameObject _reference;
    static List<MessageModal> _instances = new();

    public static MessageModal Show(string title, string message, Type type, params (string Label, Action OnClick)[] buttons)
    {
        #warning BLOCK OTHER INPUT LAYERS WHILE THIS IS ON SCREEN
        if (buttons == null || buttons.Length == 0)
            buttons = new[] { ("OK", default(Action)) };

        _reference ??= Resources.Load<GameObject>("MessageModal");
        var copy = Instantiate(_reference);
        if (copy.GetComponentInChildren<MessageModal>() is { } modal == false)
            throw new InvalidOperationException($"Could not find component {nameof(MessageModal)} in MessageModal asset");

        DontDestroyOnLoad(copy.gameObject);
        modal.Button.gameObject.SetActive(false);
        modal.Header.text = title;
        modal.Header.GetComponentInParent<Image>().color = type switch
        {
            Type.Error => new Color(1.0f, 0.5f, 0.5f),
            Type.Warning => new Color(1.0f, 0.75f, 0.25f),
            Type.Message => new Color(0.5f, 0.5f, 0.5f),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        modal.Message.text = message;
        modal.GetComponent<Canvas>().sortingOrder = _instances.Count > 0 ? _instances[^1].GetComponent<Canvas>().sortingOrder - 1 : 32767; // sort by how recent it is
        _instances.Add(modal);
        foreach (var (label, action) in buttons)
        {
            var otherButton = Instantiate(modal.Button.gameObject, modal.Button.transform.parent);
            otherButton.SetActive(true);
            otherButton.GetComponentInChildren<TMPro.TMP_Text>().text = label;
            otherButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                action?.Invoke();
                Destroy(modal.gameObject);
                _instances.Remove(modal);
            });
        }

        return modal;
    }

    public enum Type
    {
        Error,
        Warning,
        Message
    }
}