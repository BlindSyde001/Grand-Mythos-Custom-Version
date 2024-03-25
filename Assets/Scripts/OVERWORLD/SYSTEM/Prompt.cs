using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Prompt : MonoBehaviour
{
    static Prompt PromptTemplate;

    public TMP_Text Text;

    void Awake()
    {
        if (PromptTemplate == null)
            PromptTemplate = this;

        gameObject.SetActive(false);
    }

    void Update()
    {
        ClearPrompts();
    }

    void OnDestroy()
    {
        _promptCache.Remove(this);
        _promptInUse.Remove(this);
    }

    static Prompt()
    {
        DomainReloadHelper.BeforeReload += helper =>
        {
            foreach (Prompt prompt in _promptCache)
                Destroy(prompt.gameObject);
            foreach (Prompt prompt in _promptInUse)
                Destroy(prompt.gameObject);

            helper.PromptTemplate = PromptTemplate;
        };
        DomainReloadHelper.AfterReload += helper => PromptTemplate = helper.PromptTemplate;
    }

    static int _lastClear;
    static List<Prompt> _promptCache = new List<Prompt>();
    static List<Prompt> _promptInUse = new List<Prompt>();

    public static void TryShowPromptThisFrame(Vector3 worldPosition, string text)
    {
        if (_lastClear != Time.frameCount)
            ClearPrompts();

        if (_promptInUse.Count > 16)
            return;

        Prompt prompt;
        if (_promptCache.Count == 0)
        {
            prompt = Instantiate(PromptTemplate, PromptTemplate.transform.parent);
            prompt.transform.SetSiblingIndex(PromptTemplate.transform.GetSiblingIndex());
        }
        else
        {
            prompt = _promptCache[^1];
            _promptCache.RemoveAt(_promptCache.Count - 1);
        }

        prompt.gameObject.SetActive(true);
        _promptInUse.Add(prompt);
        prompt.Text.text = text;
        prompt.transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPosition);
    }

    static void ClearPrompts()
    {
        _lastClear = Time.frameCount;
        foreach (var prompt in _promptInUse)
            prompt.gameObject.SetActive(false);
        _promptCache.AddRange(_promptInUse);
        _promptInUse.Clear();
    }
}

public partial class DomainReloadHelper
{
    public Prompt PromptTemplate;
}