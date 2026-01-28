using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Playable Characters Collection")]
public class PlayableCharacters : ScriptableObject
{
    static PlayableCharacters? __instance;

    static PlayableCharacters Instance
    {
        get
        {
            __instance ??= Resources.Load<PlayableCharacters>("PlayableCharacters");
#if UNITY_EDITOR
            if (__instance == null)
            {
                Debug.LogError("Could not load PlayableCharacters resource - automatically creating an instance in 'Resources/'");
                __instance = CreateInstance<PlayableCharacters>();
                if (UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources") == false)
                    UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                UnityEditor.AssetDatabase.CreateAsset(__instance, "Assets/Resources/PlayableCharacters.asset");
                UnityEditor.AssetDatabase.SaveAssets();
            }
#endif

            return __instance;
        }
    }

    [SerializeField] List<HeroExtension> Characters = new();

    public static bool TryGet(guid guid, out HeroExtension? hero)
    {
        if (guid == default)
        {
            hero = null;
            return true;
        }

        foreach (var heroExtension in Instance.Characters)
        {
            if (heroExtension.Guid == guid)
            {
                hero = heroExtension;
                return true;
            }
        }

        hero = default;
        return false;
    }

    public static void EnsureRegistered(HeroExtension newHero)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += FixupThisIdentifiable;

        void FixupThisIdentifiable()
        {
            if (newHero.Guid == default)
            {
                Debug.LogException(new InvalidOperationException($"{newHero} has an unset GUID, this is invalid"), newHero);
                return;
            }

            lock (Instance.Characters)
            {
                foreach (var heroExtension in Instance.Characters)
                    if (heroExtension.Guid == newHero.Guid)
                        return;

                Instance.Characters.Add(newHero);
            }

            UnityEditor.EditorUtility.SetDirty(Instance);
        }
#endif
    }
}