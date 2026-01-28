using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DG.Tweening;
using Menu_Containers.Menu_Item_Actions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class SettingsMenuActions : MenuContainer
{
    public required SettingsMenuDropdownTemplate DropdownTemplate;
    public required RectTransform DefaultDropdownParent;
    public SerializableDictionary<string, SettingsMenuDropdownTemplate> Dropdowns = new();

    public required TMP_Dropdown ResolutionDropdown;
    public required TMP_Dropdown BattleSpeedDropdown;

    readonly Dictionary<object, Action> _scheduledChanges = new();

    public void ApplyChanges()
    {
        foreach (var change in _scheduledChanges)
            change.Value();
        _scheduledChanges.Clear();

        Settings.Current.Width = Screen.width;
        Settings.Current.Height = Screen.height;
        Settings.Current.RefreshRateNumerator = Screen.currentResolution.refreshRateRatio.numerator;
        Settings.Current.RefreshRateDenominator = Screen.currentResolution.refreshRateRatio.denominator;
        Settings.Current.WindowMode = Screen.fullScreenMode;
        Settings.SaveToDisk();
    }

    public void DiscardChanges()
    {
        _scheduledChanges.Clear();
        ResetDisplay();
    }

    void ResetDisplay()
    {
        ResolutionDropdown.options.Clear();
        var currentResolution = Screen.currentResolution;
        var resolutions = Screen.resolutions;
        Array.Reverse(resolutions);
        for (int i = 0; i < resolutions.Length; i++)
        {
            var resolution = resolutions[i];
            ResolutionDropdown.options.Add(new($"{resolution.width}x{resolution.height} : {resolution.refreshRateRatio}hz"));
            if (ResolutionMatch(resolution, currentResolution))
                ResolutionDropdown.value = i;
        }
        ResolutionDropdown.onValueChanged.RemoveAllListeners();
        ResolutionDropdown.onValueChanged.AddListener(index =>
        {
            _scheduledChanges[ResolutionDropdown] = () =>
            {
                var resolution = resolutions[index];
                Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRateRatio);
            };
        });

        var regex = new Regex("([A-Z]+)");
        
        var fields = typeof(Settings).GetFields(BindingFlags.Instance | BindingFlags.Public).ToDictionary(x => x.Name, x => x);
        foreach (var (name, data) in Dropdowns)
        {
            data.Dropdown.options.Clear();
            data.Dropdown.onValueChanged.RemoveAllListeners();

            var field = fields[name];
            var current = field.GetValue(Settings.Current);
            var values = Enum.GetValues(field.FieldType);
            foreach (var value in values)
            {
                var str = value.ToString();
                str = regex.Replace(str, " $1").TrimStart();

                data.Dropdown.options.Add(new(str));
                if (value == current)
                    data.Dropdown.value = data.Dropdown.options.Count - 1;
            }
            data.Dropdown.onValueChanged.AddListener(i => { field.SetValue(Settings.Current, values.GetValue(i)); });
        }

        // Special handling for WindowMode
        var windowModeDropdown = Dropdowns[nameof(Settings.WindowMode)].Dropdown;
        windowModeDropdown.onValueChanged.AddListener(index =>
        {
            _scheduledChanges[windowModeDropdown] = () => Screen.fullScreenMode = Settings.Current.WindowMode;
        });

        /*{
            var speeds = new[]
            {
                (name:"50%", value:0.5f),
                (name:"75%", value:0.75f),
                (name:"100%", value:1.0f),
                (name:"125%", value:1.25f),
                (name:"150%", value:1.5f)
            };
            SetupDropdown(BattleSpeedDropdown, speeds, Settings.Current.BattleSpeed, f => Settings.Current.BattleSpeed = f);
        }*/

        void SetupDropdown<T>(TMP_Dropdown dropdown, (string name, T associatedValue)[] values, T selectedValue, Action<T> onValueChanged) where T : notnull
        {
            dropdown.options.Clear();
            for (int i = 0; i < values.Length; i++)
            {
                var speed = values[i];
                dropdown.options.Add(new(speed.name));
                if (selectedValue.Equals(speed.associatedValue))
                    dropdown.value = i;
            }
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.onValueChanged.AddListener(index =>
            {
                _scheduledChanges[dropdown] = () => onValueChanged(values[index].associatedValue);
            });
        }
    }


    static bool ResolutionMatch(Resolution a, Resolution b)
    {
        return a.height == b.height
               && a.width == b.width
               && a.refreshRateRatio.denominator == b.refreshRateRatio.denominator
               && a.refreshRateRatio.numerator == b.refreshRateRatio.numerator;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _scheduledChanges.Clear();
        ResetDisplay();
    }

    public override IEnumerable Open(MenuInputs menuInputs)
    {
        gameObject.SetActive(true);

        if (gameObject.TryGetComponent<CanvasGroup>(out var canvas) == false)
            canvas = gameObject.AddComponent<CanvasGroup>();
        canvas.alpha = 0f;
        canvas.DOFade(1f, menuInputs.Speed);

        yield return new WaitForSeconds(menuInputs.Speed);
    }
    public override IEnumerable Close(MenuInputs menuInputs)
    {
        if (gameObject.TryGetComponent<CanvasGroup>(out var canvas) == false)
            canvas = gameObject.AddComponent<CanvasGroup>();
        canvas.alpha = 1f;
        canvas.DOFade(0f, menuInputs.Speed);

        yield return new WaitForSeconds(menuInputs.Speed);

        gameObject.SetActive(false);
    }

    #if UNITY_EDITOR
    [Button]
    void Regenerate()
    {
        foreach (var fieldInfo in typeof(Settings).GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            if (fieldInfo.FieldType.IsEnum == false)
                continue;

            if (Dropdowns.ContainsKey(fieldInfo.Name))
                continue;

            var template = (SettingsMenuDropdownTemplate)UnityEditor.PrefabUtility.InstantiatePrefab(DropdownTemplate, DefaultDropdownParent);
            template.name = template.Label.text = UnityEditor.ObjectNames.NicifyVariableName(fieldInfo.Name);
            Dropdowns[fieldInfo.Name] = template;
        }
    }
    #endif
}

public partial class Settings
{
    public int Width, Height;
    public FullScreenMode WindowMode;
    public uint RefreshRateNumerator;
    public uint RefreshRateDenominator;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void InitResolution()
    {
        TryLoadFromDisk();
        int width = Current.Width, height = Current.Height;
        uint num = Current.RefreshRateNumerator, denom = Current.RefreshRateDenominator;
        foreach (var resolution in Screen.resolutions)
        {
            if (resolution.width == width
                && resolution.height == height
                && resolution.refreshRateRatio.numerator == num
                && resolution.refreshRateRatio.denominator == denom)
            {
                Screen.SetResolution(width, height, Current.WindowMode, resolution.refreshRateRatio);
                break;
            }
        }
    }
}