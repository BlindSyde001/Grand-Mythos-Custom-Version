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
    internal List<ItemCapsule> ItemsDropped = new();
    public List<TextMeshProUGUI> ItemRewardsText;

    [Header("Audio and Video")]
    public VideoAnimation VideoAnimator;

    public GameObject LosePanel;

    // UPDATES
    void Start()
    {
        gameManager = GameManager._instance;
    }

    void Update()
    {
        if (activateUpdate)
        {
            for (int i = 0; i < GameManager._instance._PartyLineup.Count; i++)         // Setting UI Numbers
            {
                HeroPanels[i].displayLevel.text = gameManager._PartyLineup[i].Level.ToString();
                HeroPanels[i].displayEXPBar.fillAmount = (float)(gameManager._PartyLineup[i]._TotalExperience - gameManager._PartyLineup[i].PrevExperienceThreshold)/ 
                                                                (gameManager._PartyLineup[i].ExperienceThreshold - gameManager._PartyLineup[i].PrevExperienceThreshold);
                HeroPanels[i].displayEXPToNextLevel.text = (gameManager._PartyLineup[i].ExperienceThreshold - gameManager._PartyLineup[i]._TotalExperience).ToString();
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
        for (int i = 0; i < GameManager._instance._PartyLineup.Count; i++)         // Setting UI Numbers
        {
            HeroPanels[i].gameObject.SetActive(true);
            HeroPanels[i].displayBanner.sprite = gameManager._PartyLineup[i].charBanner;
            HeroPanels[i].displayName.text = gameManager._PartyLineup[i].charName;
            HeroPanels[i].displayLevel.text = gameManager._PartyLineup[i].Level.ToString();
            HeroPanels[i].displayEXPBar.fillAmount = (float)gameManager._PartyLineup[i]._ExperienceToNextLevel / gameManager._PartyLineup[i].ExperienceThreshold;
            HeroPanels[i].displayEXPToNextLevel.text = (gameManager._PartyLineup[i].ExperienceThreshold - gameManager._PartyLineup[i]._TotalExperience).ToString();
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
            #warning would be nice to remove the cast/test here
            if (unit.Team.Allies.Contains(battle.PlayerTeam) == false && unit is EnemyExtension enemy)
            {
                sharedExp += enemy.experiencePool;
                creditsEarned += enemy.creditPool;
            }
        }

        List<HeroExtension> heroesAlive = new();
        foreach (var unit in battle.Units)
        {
#warning would be nice to remove the cast/test here
            if (unit._CurrentHP > 0 && unit.Team == battle.PlayerTeam && unit is HeroExtension hero)
                heroesAlive.Add(hero);
        }

        Debug.Assert(heroesAlive.Count != 0);

        int individualExperience = sharedExp / heroesAlive.Count;
        ExperienceRewards.text = $"Experience: {sharedExp}  ({individualExperience})"; ;
        CurrencyRewards.text = $"Credits: {creditsEarned}";

        foreach (var hero in heroesAlive)
            StartCoroutine(ReceiveExperienceRewards(hero, individualExperience, duration));

        activateUpdate = true;
        InventoryManager._instance.creditsInBag += creditsEarned;
        
        yield return new WaitForSeconds(duration * 2);

        foreach (PartyContainer a in HeroPanels)
            a.gameObject.SetActive(false);

        ItemRewardPanel.SetActive(true);

        // Grab all dropped items
        foreach (CharacterTemplate unit in battle.Units)
        {
            if (unit is not EnemyExtension enemy)
                continue;

            for (int j = 0; j < enemy.DropItems.Count; j++)
            {
                if (Random.Range(0, 100) < enemy.DropRate[j])
                    ItemsDropped.Add(enemy.DropItems[j]);
            }
        }

        int max = ItemsDropped.Count;
        if (max > ItemRewardsText.Count)
        {
            Debug.LogError($"Not enough reward text for the amount of items provided ({ItemsDropped.Count}/{ItemRewardsText.Count})");
            max = ItemRewardsText.Count;
        }

        // Write em in the UI
        for (int i = 0; i < max; i++)
            ItemRewardsText[i].text = $"{ItemsDropped[i].ItemAmount} x {ItemsDropped[i].thisItem.name}";

        foreach (ItemCapsule item in ItemsDropped)
            InventoryManager._instance.AddToInventory(item);

        yield return new WaitForSeconds(duration);

        ReturnToOverworld(battle);
    }

    static IEnumerator ReceiveExperienceRewards(HeroExtension myHero, int individualExperience, float duration)
    {
        int start = myHero._TotalExperience;
        float t = 0f;
        while (t < 1f)
        {
            myHero._TotalExperience = (int)Mathf.Lerp(start, start + individualExperience, t);
            t += Time.deltaTime / duration;
            myHero.LevelUpCheck();
            yield return null;
        }
        myHero._TotalExperience = start + individualExperience;
    }

    static void ReturnToOverworld(BattleStateMachine battle)
    {
        foreach (var hero in battle.PartyLineup)
            hero._CurrentHP = hero._CurrentHP == 0 ? 1 : hero._CurrentHP;
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
