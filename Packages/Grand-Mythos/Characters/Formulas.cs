﻿using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Gameplay")]
public class Formulas : ScriptableObject
{
    [BoxGroup("Flow"), InfoBox("The amount of flow gained per points of damage the unit deals")]
    public float SourceFlowScaler = 1f;
    [BoxGroup("Flow"), InfoBox("The amount of flow gained per points of damage the unit receives")]
    public float TargetFlowScaler = 1f;
    [BoxGroup("Flow"), InfoBox("The amount of flow lost per seconds of battle time for a unit in flow state")]
    public float FlowDepletionRate = 0f;

    [FormerlySerializedAs("PercentChancePerStat")]
    [InfoBox("Height is the percent chance, width is the amount of stats required to attain this percentage")]
    [HideLabel, CustomValueDrawer(nameof(DrawBigGraph)), HorizontalGroup("Crit/HZ"), BoxGroup("Crit")]
    public AnimationCurve CritPercentChancePerStat = AnimationCurve.Linear(0, 0, 100, 100);

    [FormerlySerializedAs("DamagePerStat")]
    [InfoBox("Height is the damage multiplier, width is the amount of stats required to attain this multiplier")]
    [HideLabel, CustomValueDrawer(nameof(DrawBigGraphWithInfo)), HorizontalGroup("Crit/HZ"), BoxGroup("Crit")]
    public AnimationCurve CritDamagePerStat = AnimationCurve.Linear(0, 1, 100, 3);

    int _previewStat;

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
        _previewStat = UnityEditor.EditorGUILayout.IntSlider(_previewStat, (int)CritPercentChancePerStat[0].time, (int)CritPercentChancePerStat[CritPercentChancePerStat.length - 1].time);
#endif
        return value;
    }

    public static void GetCritModifiersBasedOnLuck(int luck, out float chance, out float damage)
    {
        var instance = SingletonManager.Instance.Formulas;
        chance = instance.CritPercentChancePerStat.Evaluate((float)luck);
        damage = instance.CritDamagePerStat.Evaluate((float)luck);
    }
}