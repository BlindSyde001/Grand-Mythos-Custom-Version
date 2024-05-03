using UnityEngine;

public class SingletonManager : ScriptableObject
{
    public CriticalFormula CriticalFormula;





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