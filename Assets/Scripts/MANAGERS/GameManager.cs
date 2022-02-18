using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }

    #region OVERWORLD DATA
    [BoxGroup("SCENE DATA")]
    [SerializeField]
    internal int _DoorwayIndex;
    [BoxGroup("SCENE DATA")]
    [SerializeField]
    internal string _LastKnownScene;
    [BoxGroup("SCENE DATA")]
    [SerializeField]
    internal Vector3 _LastKnownPosition;
    [BoxGroup("SCENE DATA")]
    [SerializeField]
    internal Quaternion _LastKnownRotation;
    #endregion
    #region BATTLE DATA
    [BoxGroup("PARTY DATA")]
    [SerializeField]
    internal List<HeroExtension> _AllPartyMembers; // All in Party
    [BoxGroup("PARTY DATA")]
    [SerializeField]
    internal List<HeroExtension> _PartyLineup;  // Who I've selected to be fighting
    [BoxGroup("PARTY DATA")]
    [SerializeField]
    internal List<HeroExtension> _ReservesLineup;  // Who I have available in the Party

    [BoxGroup("BATTLE PARTY DATA")]
    public List<HeroExtension> _PartyMembersActive;
    [BoxGroup("BATTLE PARTY DATA")]
    public List<HeroExtension> _PartyMembersDowned;


    [SerializeField]
    internal List<EnemyExtension> _EnemyLineup;
    #endregion
    #region DATABASE LIBRARY
    [TitleGroup("LIBRARY COLLECTION")]
    [HorizontalGroup("LIBRARY COLLECTION/Split")]
    [VerticalGroup("LIBRARY COLLECTION/Split/Left")]
    [BoxGroup("LIBRARY COLLECTION/Split/Left/WEAPONS")]
    public List<Weapon> _GunsDatabase;

    [TitleGroup("LIBRARY COLLECTION")]
    [VerticalGroup("LIBRARY COLLECTION/Split/Left")]
    [BoxGroup("LIBRARY COLLECTION/Split/Left/WEAPONS")]
    public List<Weapon> _WarhammersDatabase;

    [TitleGroup("LIBRARY COLLECTION")]
    [VerticalGroup("LIBRARY COLLECTION/Split/Left")]
    [BoxGroup("LIBRARY COLLECTION/Split/Left/WEAPONS")]
    public List<Weapon> _PowerGlovesDatabase;

    [TitleGroup("LIBRARY COLLECTION")]
    [VerticalGroup("LIBRARY COLLECTION/Split/Left")]
    [BoxGroup("LIBRARY COLLECTION/Split/Left/WEAPONS")]
    public List<Weapon> _GrimoiresDatabase;

    [TitleGroup("LIBRARY COLLECTION")]
    [HorizontalGroup("LIBRARY COLLECTION/Split")]
    [VerticalGroup("LIBRARY COLLECTION/Split/Right")]
    [BoxGroup("LIBRARY COLLECTION/Split/Right/ARMOUR")]
    public List<Armour> _LeatherDatabase;

    [TitleGroup("LIBRARY COLLECTION")]
    [VerticalGroup("LIBRARY COLLECTION/Split/Right")]
    [BoxGroup("LIBRARY COLLECTION/Split/Right/ARMOUR")]
    public List<Armour> _MailDatabase;

    [TitleGroup("LIBRARY COLLECTION")]
    [VerticalGroup("LIBRARY COLLECTION/Split/Right")]
    [BoxGroup("LIBRARY COLLECTION/Split/Right/ARMOUR")]
    public List<Armour> _ChasisDatabase;

    [TitleGroup("LIBRARY COLLECTION")]
    [VerticalGroup("LIBRARY COLLECTION/Split/Right")]
    [BoxGroup("LIBRARY COLLECTION/Split/Right/ARMOUR")]
    public List<Armour> _RobesDatabase;

    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/ACCESSORIES")]
    public List<Accessory> _AccessoryDatabase;

    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/ITEMS")]
    public List<Consumable> _ConsumablesDatabase;

    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/ITEMS")]
    public List<KeyItem> _KeyItemsDatabase;

    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/ITEMS")]
    public List<Loot> _LootDatabase;

    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/ACTIONS")]
    public List<Action> _HeroSkillsDatabase;

    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/ACTIONS")]
    public List<Action> _ItemSkillsDatabase;

    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/CONDITIONS")]
    public List<Condition> _ConditionsDatabase;
    #endregion
}
