using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    [SerializeField]
    internal List<EnemyExtension> _EnemyLineup;
    #endregion
    #region DATABASE LIBRARY
    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/WEAPONS")]
    public List<Gun> _GunsDatabase;
    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/WEAPONS")]
    public List<Warhammer> _WarhammersDatabase;
    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/WEAPONS")]
    public List<PowerGlove> _PowerGlovesDatabase;
    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/WEAPONS")]
    public List<Grimoire> _GrimoiresDatabase;

    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/ARMOUR")]
    public List<Armour> _LeatherDatabase;
    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/ARMOUR")]
    public List<Armour> _MailDatabase;
    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/ARMOUR")]
    public List<Armour> _ChasisDatabase;
    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/ARMOUR")]
    public List<Armour> _RobesDatabase;

    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/ACCESSORIES")]
    public List<Accessory> _AccessoryDatabase;

    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/ABILITIES")]
    public List<Action> _SkillsDatabase;
    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/ABILITIES")]
    public List<Action> _MagicDatabase;

    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/ITEMS")]
    public List<Consumable> _ConsumablesDatabase;

    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/ITEMS")]
    public List<Key> _KeyItemsDatabase;

    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/CONDITIONS")]
    public List<Condition> _AllyConditionDatabase;
    [TitleGroup("LIBRARY COLLECTION")]
    [BoxGroup("LIBRARY COLLECTION/CONDITIONS")]
    public List<Condition> _EnemyConditionDatabase;
    #endregion
}
