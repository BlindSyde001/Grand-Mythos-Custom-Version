using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class CriticalFormula : ScriptableObject
{
    [InfoBox("Height is the percent chance, width is the amount of stats required to attain this percentage")]
    [HideLabel, CustomValueDrawer(nameof(DrawBigGraph)), HorizontalGroup]
    public AnimationCurve PercentChancePerStat = AnimationCurve.Linear(0, 0, 100, 100);

    [InfoBox("Height is the damage multiplier, width is the amount of stats required to attain this multiplier")]
    [HideLabel, CustomValueDrawer(nameof(DrawBigGraphWithInfo)), HorizontalGroup]
    public AnimationCurve DamagePerStat = AnimationCurve.Linear(0, 1, 100, 3);

    int _previewStat;

    void OnEnable()
    {
        if (SingletonManager.Instance.CriticalFormula == null)
            SingletonManager.Instance.CriticalFormula = this;
        else if (ReferenceEquals(SingletonManager.Instance.CriticalFormula, this) == false)
            Debug.LogError($"Multiple instances of {nameof(CriticalFormula)} is not allowed");
    }

    AnimationCurve DrawBigGraph(AnimationCurve value)
    {
        #if UNITY_EDITOR
        value = UnityEditor.EditorGUILayout.CurveField(value, GUILayout.MinHeight(160));
        #endif
        return value;
    }

    AnimationCurve DrawBigGraphWithInfo(AnimationCurve value)
    {
#if UNITY_EDITOR
        value = UnityEditor.EditorGUILayout.CurveField(value, GUILayout.MinHeight(160));
        GetCritModifiersBasedOnLuck(_previewStat, out var chance, out var damage);
        UnityEditor.EditorGUILayout.SelectableLabel($"Preview: {_previewStat} luck -> {chance:F}% chance of a {damage}x critical hit");
        _previewStat = UnityEditor.EditorGUILayout.IntSlider(_previewStat, (int)PercentChancePerStat[0].time, (int)PercentChancePerStat[PercentChancePerStat.length - 1].time);
#endif
        return value;
    }

    public static void GetCritModifiersBasedOnLuck(int luck, out float chance, out float damage)
    {
        var instance = SingletonManager.Instance.CriticalFormula;
        chance = instance.PercentChancePerStat.Evaluate((float)luck);
        damage = instance.DamagePerStat.Evaluate((float)luck);
    }
}