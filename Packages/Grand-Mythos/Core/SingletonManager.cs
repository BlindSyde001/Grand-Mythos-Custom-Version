﻿using Nodalog;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class SingletonManager : ScriptableObject
{
    [FormerlySerializedAs("CriticalFormula")] [Required] public Formulas Formulas;
    [Required] public InputActionAsset PlayerInput;
    [Required] public Tally HunterRank, HunterExperience, Credits;
    [Required] public EventSystem DefaultEventSystem;





    static SingletonManager __instance;

    public static SingletonManager Instance
    {
        get
        {
            __instance ??= Resources.Load<SingletonManager>("SingletonManager");
#if UNITY_EDITOR
            if (__instance == null)
            {
                Debug.LogError("Could not load SingletonManager resource - automatically creating an instance in 'Resources/'");
                __instance = CreateInstance<SingletonManager>();
                if (UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources") == false)
                    UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                UnityEditor.AssetDatabase.CreateAsset(__instance, "Assets/Resources/SingletonManager.asset");
                UnityEditor.AssetDatabase.SaveAssets();
            }
#endif

            return __instance;
        }
    }
}