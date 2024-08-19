using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class SettingsMenuActions : MenuContainer
{
    [Required] public TMP_Dropdown ResolutionDropdown;
    [Required] public TMP_Dropdown WindowModeDropdown;
    [Required] public TMP_Dropdown BattleSpeedDropdown;
    [Required] public TMP_Dropdown BattleCommandSpeedDropdown;
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

        WindowModeDropdown.options.Clear();
        var modes = new[]
        {
            (name:"Exclusive FullScreen", value:FullScreenMode.ExclusiveFullScreen),
            (name:"FullScreen Window", value:FullScreenMode.FullScreenWindow),
            (name:"Maximized Window", value:FullScreenMode.MaximizedWindow),
            (name:"Windowed", value:FullScreenMode.Windowed)
        };
        for (int i = 0; i < modes.Length; i++)
        {
            var fullScreenMode = modes[i];
            WindowModeDropdown.options.Add(new(fullScreenMode.name));
            if (Screen.fullScreenMode == fullScreenMode.value)
                WindowModeDropdown.value = i;
        }
        WindowModeDropdown.onValueChanged.RemoveAllListeners();
        WindowModeDropdown.onValueChanged.AddListener(index =>
        {
            _scheduledChanges[WindowModeDropdown] = () => Screen.fullScreenMode = modes[index].value;
        });

        {
            var speeds = new[]
            {
                (name:"50%", value:0.5f),
                (name:"75%", value:0.75f),
                (name:"100%", value:1.0f),
                (name:"125%", value:1.25f),
                (name:"150%", value:1.5f)
            };
            SetupDropdown(BattleSpeedDropdown, speeds, Settings.Current.BattleSpeed, f => Settings.Current.BattleSpeed = f);
        }

        {
            var values = Enum.GetValues(typeof(BattleMenuMode));
            var dropdownData = new (string, BattleMenuMode)[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                var val = (BattleMenuMode)values.GetValue(i);
                dropdownData[i] = (val.ToString(), val);
            }
            SetupDropdown(BattleCommandSpeedDropdown, dropdownData, Settings.Current.BattleMenuMode, f => Settings.Current.BattleMenuMode = f);
        }
    }

    void SetupDropdown<T>(TMP_Dropdown dropdown, (string name, T associatedValue)[] values, T selectedValue, Action<T> onValueChanged)
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
        yield return new WaitForSeconds(menuInputs.Speed);
    }
    public override IEnumerable Close(MenuInputs menuInputs)
    {
        yield return new WaitForSeconds(menuInputs.Speed);
        gameObject.SetActive(false);
    }
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