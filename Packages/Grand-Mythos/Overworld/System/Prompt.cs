using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Prompt : MonoBehaviour
{
    const int PromptLimit = 16;

    static Prompt? PromptTemplate;

    public required TMP_Text Text;

    void Awake()
    {
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
        if (PromptTemplate == this)
            PromptTemplate = null;
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
    static bool _presentedUnique;
    static List<Prompt> _promptCache = new List<Prompt>();
    static List<Prompt> _promptInUse = new List<Prompt>();

    public static void ShowPromptThisFrame(Vector3 worldPosition, string text)
    {
        if (_promptInUse.Count > PromptLimit)
            return;

        if (PromptTemplate == null)
            PromptTemplate = FindAnyObjectByType<Prompt>(FindObjectsInactive.Include);

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

    public static bool TryShowInteractivePromptThisFrame(Vector3 worldPosition, string text)
    {
        if (_lastClear != Time.frameCount)
            ClearPrompts();

        if (_presentedUnique)
            return false;

        if (_promptInUse.Count > PromptLimit)
            return false;

        _presentedUnique = true;
        ShowPromptThisFrame(worldPosition, text);
        return true;
    }

    static void ClearPrompts()
    {
        _presentedUnique = false;
        _lastClear = Time.frameCount;
        foreach (var prompt in _promptInUse)
            prompt.gameObject.SetActive(false);
        _promptCache.AddRange(_promptInUse);
        _promptInUse.Clear();
    }
}

public partial class DomainReloadHelper
{
    public Prompt? PromptTemplate;
}