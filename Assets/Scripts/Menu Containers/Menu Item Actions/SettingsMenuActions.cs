using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SettingsMenuActions : MenuContainer
{
    public TMP_Dropdown ResolutionDropdown;
    public TMP_Dropdown WindowModeDropdown;
    public TMP_Dropdown BattleSpeedDropdown;
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
            ResolutionDropdown.options.Add(new($"{resolution.width}x{resolution.height} {resolution.refreshRateRatio}"));
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

        BattleSpeedDropdown.options.Clear();
        var speeds = new[]
        {
            (name:"50%", value:0.5f),
            (name:"75%", value:0.75f),
            (name:"100%", value:1.0f),
            (name:"125%", value:1.25f),
            (name:"150%", value:1.5f)
        };
        for (int i = 0; i < speeds.Length; i++)
        {
            var speed = speeds[i];
            BattleSpeedDropdown.options.Add(new(speed.name));
            if (Settings.Current.BattleSpeed == speed.value)
                BattleSpeedDropdown.value = i;
        }
        BattleSpeedDropdown.onValueChanged.RemoveAllListeners();
        BattleSpeedDropdown.onValueChanged.AddListener(index =>
        {
            _scheduledChanges[BattleSpeedDropdown] = () => Settings.Current.BattleSpeed = speeds[index].value;
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