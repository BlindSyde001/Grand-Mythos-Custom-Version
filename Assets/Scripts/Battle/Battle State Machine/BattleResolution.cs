using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class BattleResolution : MonoBehaviour
{
    // VARIABLES
    GameManager gameManager;

    [Header("Fill In Panel Effect")]
    public Image RewardBackground;
    public Image HeroGridBackground;
    float speed = 0.5f;

    [Header("Rewards")]
    public TextMeshProUGUI ExperienceRewards;
    public TextMeshProUGUI CurrencyRewards;
    public List<PartyContainer> HeroPanels;
    bool activateUpdate;

    float duration = 3f;
    public GameObject ItemRewardPanel;
    public List<TextMeshProUGUI> ItemRewardsText;

    [Header("Audio and Video")]
    public VideoAnimation VideoAnimator;

    public GameObject LosePanel;

    // UPDATES
    void Start()
    {
        gameManager = GameManager.Instance;
    }

    void Update()
    {
        if (activateUpdate)
        {
            for (int i = 0; i < GameManager.Instance.PartyLineup.Count; i++)         // Setting UI Numbers
            {
                HeroPanels[i].displayLevel.text = gameManager.PartyLineup[i].Level.ToString();
                HeroPanels[i].displayEXPBar.fillAmount = (float)(gameManager.PartyLineup[i].Experience - gameManager.PartyLineup[i].PrevExperienceThreshold)/
                                                                (gameManager.PartyLineup[i].ExperienceThreshold - gameManager.PartyLineup[i].PrevExperienceThreshold);
                HeroPanels[i].displayEXPToNextLevel.text = (gameManager.PartyLineup[i].ExperienceThreshold - gameManager.PartyLineup[i].Experience).ToString();
            }
        }
    }

    // METHODS
    public IEnumerable ResolveBattle(bool victory, BattleStateMachine battleStateMachine)
    {
        var enumerator = VideoAnimator.PlayVideoClip(VideoAnimator.videoClips[victory ? 0 : 1]);
        while (enumerator.MoveNext())
            yield return enumerator.Current;

        if (victory)
        {
            foreach (var yields in ActivatePanel())
                yield return yields;
            foreach (var yields in DistributeRewards(battleStateMachine))
                yield return yields;
        }
        else
        {
            LosePanel.SetActive(true);
        }
    }
    IEnumerable ActivatePanel()
    {
        RewardBackground.DOFillAmount(1, speed);                                   // Cool Anim Effect
        HeroGridBackground.DOFillAmount(1, speed);                                 // Cool Anim effect
        yield return new WaitForSeconds(speed);
        for (int i = 0; i < GameManager.Instance.PartyLineup.Count; i++)         // Setting UI Numbers
        {
            HeroPanels[i].gameObject.SetActive(true);
            HeroPanels[i].displayBanner.sprite = gameManager.PartyLineup[i].Banner;
            HeroPanels[i].displayName.text = gameManager.PartyLineup[i].gameObject.name;
            HeroPanels[i].displayLevel.text = gameManager.PartyLineup[i].Level.ToString();
            HeroPanels[i].displayEXPBar.fillAmount = (float)gameManager.PartyLineup[i].ExperienceToNextLevel / gameManager.PartyLineup[i].ExperienceThreshold;
            HeroPanels[i].displayEXPToNextLevel.text = (gameManager.PartyLineup[i].ExperienceThreshold - gameManager.PartyLineup[i].Experience).ToString();
        }
        ExperienceRewards.gameObject.SetActive(true);
        CurrencyRewards.gameObject.SetActive(true);
    }

    IEnumerable DistributeRewards(BattleStateMachine battle)
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

        List<HeroExtension> heroesAlive = new();
        foreach (var unit in battle.Units)
        {
#warning would be nice to remove the cast/test here
            if (unit.Profile.CurrentHP > 0 && unit.Profile.Team == battle.PlayerTeam && unit.Profile is HeroExtension hero)
                heroesAlive.Add(hero);
        }

        Debug.Assert(heroesAlive.Count != 0);

        int individualExperience = sharedExp / heroesAlive.Count;
        ExperienceRewards.text = $"Experience: {sharedExp}  ({individualExperience})"; ;
        CurrencyRewards.text = $"Credits: {creditsEarned}";

        foreach (var hero in heroesAlive)
            StartCoroutine(ReceiveExperienceRewards(hero, individualExperience, duration));

        activateUpdate = true;
        InventoryManager.Instance.Credits += creditsEarned;
        
        yield return new WaitForSeconds(duration * 2);

        foreach (PartyContainer a in HeroPanels)
            a.gameObject.SetActive(false);

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

        yield return new WaitForSeconds(duration);

        ReturnToOverworld(battle);
    }

    static IEnumerator ReceiveExperienceRewards(HeroExtension myHero, int individualExperience, float duration)
    {
        int start = myHero.Experience;
        float t = 0f;
        while (t < 1f)
        {
            myHero.Experience = (int)Mathf.Lerp(start, start + individualExperience, t);
            t += Time.deltaTime / duration;
            myHero.LevelUpCheck();
            yield return null;
        }
        myHero.Experience = start + individualExperience;
    }

    static void ReturnToOverworld(BattleStateMachine battle)
    {
        foreach (var hero in battle.PartyLineup)
            hero.Profile.CurrentHP = hero.Profile.CurrentHP == 0 ? 1 : hero.Profile.CurrentHP;
        BattleStateMachine.ClearData();

        if (battle.gameObject.scene == SceneManager.GetActiveScene())
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i) != battle.gameObject.scene)
                    SceneManager.SetActiveScene(SceneManager.GetSceneAt(i));
            }
        }

        SceneManager.UnloadSceneAsync(battle.gameObject.scene);
    }
}
