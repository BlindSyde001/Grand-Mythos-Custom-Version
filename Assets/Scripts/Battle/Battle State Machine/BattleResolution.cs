using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

public class BattleResolution : MonoBehaviour
{
    // VARIABLES
    private GameManager gameManager;
    private PlayerControls playerControls;
    #region Fill In Panel Effect
    public Image RewardBackground;
    public Image HeroGridBackground;
    private float speed = 0.5f;
    #endregion
    #region Rewards
    public TextMeshProUGUI ExperienceRewards;
    public TextMeshProUGUI CurrencyRewards;
    public List<PartyContainer> HeroPanels;
    private bool activateUpdate;
    private int sharedExp = 0;
    private int creditsEarned = 0;

    float duration = 3f;
    public GameObject ItemRewardPanel;
    internal List<ItemCapsule> ItemsDropped = new();
    public List<TextMeshProUGUI> ItemRewardsText;
    #endregion
    #region Audio and Video
    public VideoAnimation VideoAnimator;
    #endregion

    // UPDATES
    private void Start()
    {
        playerControls = new PlayerControls();
        gameManager = GameManager._instance;
    }
    private void Update()
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
    public IEnumerator ResolveBattle(int clipNo)
    {
        StartCoroutine(VideoAnimator.PlayVideoClip(VideoAnimator.videoClips[clipNo]));
        yield return new WaitForSeconds(VideoAnimator.time);
        if (clipNo == 0)
        {
            StartCoroutine(ActivatePanel());
        }
    }
    private IEnumerator ActivatePanel()
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
        StartCoroutine(DistributeRewards());
    }

    private IEnumerator DistributeRewards()
    {
        foreach (BattleEnemyController enemy in BattleStateMachine._EnemiesDowned)
        {
            sharedExp += enemy.myEnemy.experiencePool;
            creditsEarned += enemy.myEnemy.creditPool;
        }
        ExperienceRewards.text = "Experience: " + sharedExp.ToString();
        CurrencyRewards.text = "Credits: " + creditsEarned.ToString();

        foreach (BattleHeroController hero in BattleStateMachine._HeroesActive)
        {
            StartCoroutine(ReceiveExperienceRewards(hero));
        }
        activateUpdate = true;
        InventoryManager._instance.creditsInBag += creditsEarned;
        
        yield return new WaitForSeconds(duration * 2);
        StartCoroutine(ReceiveItemRewards());
    }
    private IEnumerator ReceiveExperienceRewards(BattleHeroController hero)
    {
        int start = hero.myHero._TotalExperience;
        float t = 0f;
        while (t < 1f)
        {
            hero.myHero._TotalExperience = (int)Mathf.Lerp(start, start + sharedExp / BattleStateMachine._HeroesActive.Count, t);
            t += Time.deltaTime / duration;
            hero.myHero.LevelUpCheck();
            yield return null;
        }
        yield return new WaitForSeconds(duration);
    }
    private IEnumerator ReceiveItemRewards()
    {
        foreach (PartyContainer a in HeroPanels)
        {
            a.gameObject.SetActive(false);
        }
        ItemRewardPanel.SetActive(true);

        // Grab all dropped items
        foreach (BattleEnemyController enemy in BattleStateMachine._EnemiesDowned)
        {
            for (int j = 0; j < enemy.myEnemy.DropItems.Count; j++)
            {
                bool dropped = DidItemDrop(enemy.myEnemy.DropRate[j]);
                if (dropped == true)
                {
                    ItemsDropped.Add(enemy.myEnemy.DropItems[j]);
                }
            }
        }
        // Write em in the UI
        for (int i = 0; i < ItemsDropped.Count; i++)
        {
            ItemRewardsText[i].text = ItemsDropped[i].ItemAmount.ToString() + " x " + ItemsDropped[i].thisItem._ItemName;
        }

        foreach (ItemCapsule item in ItemsDropped)
        {
            InventoryManager._instance.AddToInventory(item);
        }
        yield return new WaitForSeconds(duration);

        ReturnToOverworld();
    }

    private static void ReturnToOverworld()
    {
        foreach(BattleHeroController hero in BattleStateMachine._HeroesDowned)
        {
            hero.myHero._CurrentHP = 1;
        }
        BattleStateMachine.ClearData();
        Cursor.visible = true;
        EventManager._instance.SwitchNewScene(2);
    }

    private bool DidItemDrop(float dropChance)
    {
        float f = Random.Range(0, 101);
        if (f <= dropChance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
