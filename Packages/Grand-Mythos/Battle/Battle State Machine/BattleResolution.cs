using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class BattleResolution : MonoBehaviour
{
    // VARIABLES
    [Header("Fill In Panel Effect")]
    public required Image RewardBackground;
    public required Image HeroGridBackground;
    private float speed = 0.5f;

    [Header("Rewards")]
    public required TextMeshProUGUI ExperienceRewards;
    public required TextMeshProUGUI CurrencyRewards;
    public List<PartyContainer> HeroPanels = new();
    private bool activateUpdate;

    private float duration = 3f;
    public required GameObject ItemRewardPanel;
    public List<TextMeshProUGUI> ItemRewardsText = new();

    [Header("Audio and Video")]
    public required VideoAnimation VideoAnimator;

    public required GameObject LosePanel;

    public UnityEvent? OnWin;
    public UnityEvent? OnLose;

    private void Update()
    {
        if (activateUpdate)
        {
            for (int i = 0; i < GameManager.Instance.PartyLineup.Count; i++)         // Setting UI Numbers
            {
                HeroPanels[i].displayLevel.text = GameManager.Instance.PartyLineup[i].Level.ToString();
                HeroPanels[i].displayEXPBar.fillAmount = (float)(GameManager.Instance.PartyLineup[i].Experience - GameManager.Instance.PartyLineup[i].PrevExperienceThreshold)/
                                                                (GameManager.Instance.PartyLineup[i].ExperienceThreshold - GameManager.Instance.PartyLineup[i].PrevExperienceThreshold);
                HeroPanels[i].displayEXPToNextLevel.text = (GameManager.Instance.PartyLineup[i].ExperienceThreshold - GameManager.Instance.PartyLineup[i].Experience).ToString();
            }
        }
    }

    // METHODS
    public async UniTask ResolveBattle(bool victory, BattleStateMachine battleStateMachine, CancellationToken cancellation)
    {
        if (victory)
            OnWin?.Invoke();
        else
            OnLose?.Invoke();
        await VideoAnimator.PlayVideoClip(VideoAnimator.videoClips[victory ? 0 : 1], cancellation);

        if (victory)
        {
            await ActivatePanel(cancellation);
            await DistributeRewards(battleStateMachine, cancellation);
        }
        else
        {
            LosePanel.SetActive(true);
        }
    }

    private async UniTask ActivatePanel(CancellationToken cancellation)
    {
        RewardBackground.DOFillAmount(1, speed);                                   // Cool Anim Effect
        HeroGridBackground.DOFillAmount(1, speed);                                 // Cool Anim effect
        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: cancellation);
        for (int i = 0; i < GameManager.Instance.PartyLineup.Count; i++)         // Setting UI Numbers
        {
            HeroPanels[i].gameObject.SetActive(true);
            HeroPanels[i].displayBanner.sprite = GameManager.Instance.PartyLineup[i].Banner;
            HeroPanels[i].displayName.text = GameManager.Instance.PartyLineup[i].name;
            HeroPanels[i].displayLevel.text = GameManager.Instance.PartyLineup[i].Level.ToString();
            HeroPanels[i].displayEXPBar.fillAmount = (float)GameManager.Instance.PartyLineup[i].ExperienceToNextLevel / GameManager.Instance.PartyLineup[i].ExperienceThreshold;
            HeroPanels[i].displayEXPToNextLevel.text = (GameManager.Instance.PartyLineup[i].ExperienceThreshold - GameManager.Instance.PartyLineup[i].Experience).ToString();
        }
        ExperienceRewards.gameObject.SetActive(true);
        CurrencyRewards.gameObject.SetActive(true);
    }

    private async UniTask DistributeRewards(BattleStateMachine battle, CancellationToken cancellation)
    {
        int sharedExp = 0;
        int creditsEarned = 0;
        foreach (var unit in battle.Units)
        {
            if (unit.Profile.Team.Allies.Contains(battle.PlayerTeam) == false)
            {
                sharedExp += unit.Profile.ExperiencePool;
                creditsEarned += unit.Profile.CreditPool;
            }
        }

        var heroesAlive = new List<CharacterTemplate>();
        foreach (var unit in battle.Units)
        {
            if (unit.Profile.CurrentHP > 0 && unit.Profile.Team == battle.PlayerTeam)
                heroesAlive.Add(unit.Profile);
        }

        Debug.Assert(heroesAlive.Count != 0);

        int individualExperience = sharedExp / heroesAlive.Count;
        ExperienceRewards.text = $"Experience: {sharedExp}  ({individualExperience})";
        CurrencyRewards.text = $"Credits: {creditsEarned}";

        foreach (var hero in heroesAlive)
            StartCoroutine(ReceiveExperienceRewards(hero, individualExperience, duration));

        activateUpdate = true;
        InventoryManager.Instance.Credits += creditsEarned;

        await UniTask.Delay(TimeSpan.FromSeconds(duration * 2), cancellationToken: cancellation);

        foreach (var container in HeroPanels)
            container.gameObject.SetActive(false);

        ItemRewardPanel.SetActive(true);

        var itemsDropped = new List<(BaseItem item, uint count)>();
        // Grab all dropped items
        foreach (var unit in battle.Units)
        {
            if (unit.Profile is HeroExtension)
                continue;

            foreach (var drop in unit.Profile.DropItems)
            {
                if (Random.Range(0, 100) < drop.DropRatePercent)
                    itemsDropped.Add((drop.Item, drop.Count));
            }
        }

        int max = itemsDropped.Count;
        if (max > ItemRewardsText.Count)
        {
            Debug.LogError($"Not enough reward text for the amount of items provided ({itemsDropped.Count}/{ItemRewardsText.Count})");
            max = ItemRewardsText.Count;
        }

        // Write em in the UI
        for (int i = 0; i < max; i++)
            ItemRewardsText[i].text = $"{itemsDropped[i].count} x {itemsDropped[i].item.name}";

        foreach (var drop in itemsDropped)
            InventoryManager.Instance.AddToInventory(drop.item, drop.count);

        await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: cancellation);
    }

    private static IEnumerator ReceiveExperienceRewards(CharacterTemplate myHero, int individualExperience, float duration)
    {
        uint start = myHero.Experience;
        float t = 0f;
        while (t < 1f)
        {
            myHero.Experience = (uint)Mathf.Lerp(start, start + individualExperience, t);
            t += Time.deltaTime / duration;
            myHero.LevelUpCheck();
            yield return null;
        }
        myHero.Experience = (uint)(start + individualExperience);
    }
}
