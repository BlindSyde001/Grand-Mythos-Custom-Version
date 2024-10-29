using System.Collections;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DetailedInfoPanel : MonoBehaviour
{
    [Required] public Animation Animation;
    [BoxGroup("Inputs")]
    [Required] public InputActionReference Close, NextProfile, PreviousProfile;
    [BoxGroup("Base")]
    [Required] public Image Profile;
    [BoxGroup("Base")]
    [Required] public TMP_Text Level, Name;
    [BoxGroup("Stats")]
    [Required] public TMP_Text Health, Mana, Attack, MagicAttack, Defense, MagicDefense, Speed, Luck;
    [BoxGroup("Resistances")]
    [Required] public TMP_Text Fire, Ice, Lightning, Water;

    public IEnumerable OpenAndAwaitClose(CharacterTemplate[] profiles)
    {
        try
        {
            enabled = true;
            gameObject.SetActive(true);
            Animation.Play(PlayMode.StopAll);
            foreach (UnityEngine.AnimationState state in Animation)
            {
                state.speed = 1f;
                state.time = 0f;
            }

            int currentProfile = 0;
            do
            {
                yield return null;

                if (NextProfile.action.WasPerformedThisFrameUnique())
                    currentProfile++;
                if (PreviousProfile.action.WasPerformedThisFrameUnique())
                    currentProfile--;
                currentProfile = currentProfile < 0
                    ? profiles.Length + currentProfile
                    : currentProfile % profiles.Length;

                var profile = profiles[currentProfile];
                if (profile is HeroExtension hero)
                    Profile.sprite = hero.Banner ?? hero.Portrait;
                else
                    Profile.sprite = profile.Portrait;

                var stats = profile.EffectiveStats;

                Level.text = profile.Level.ToString();
                Name.text = profile.Name;
                Health.text = $"{profile.CurrentHP}/{stats.HP}";
                Mana.text = $"{profile.CurrentMP}/{stats.MP}";
                Attack.text = stats.Attack.ToString();
                MagicAttack.text = stats.MagAttack.ToString();
                Defense.text = stats.Defense.ToString();
                MagicDefense.text = stats.MagDefense.ToString();
                Speed.text = stats.Speed.ToString();
                Luck.text = stats.Luck.ToString();
                Fire.text = profile.ResistanceFire.ToString();
                Ice.text = profile.ResistanceIce.ToString();
                Lightning.text = profile.ResistanceLightning.ToString();
                Water.text = profile.ResistanceWater.ToString();
            } while (Close.action.WasPerformedThisFrameUnique() == false);

            Animation.Play(PlayMode.StopAll);
            foreach (UnityEngine.AnimationState state in Animation)
            {
                state.speed = -1f;
                state.time = state.clip.length;
                yield return new WaitForSeconds(state.clip.length);
                break;
            }
        }
        finally
        {
            gameObject.SetActive(false);
        }
    }
}
