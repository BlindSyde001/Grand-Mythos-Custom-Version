using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using UnityEngine;
using Sirenix.OdinInspector;

public class GameManager : MonoBehaviour, ISaved<GameManager, GameManager.SaveV3>
{
    public static readonly guid Guid = new("bb05002e-f0d5-4936-a986-c47a045e58d8");

    public static GameManager Instance { get; private set; } = null!;

    [BoxGroup("PARTY DATA")]
    public List<HeroExtension> PartyLineup = new();  // Who I've selected to be fighting
    [BoxGroup("PARTY DATA")]
    public List<HeroExtension> ReservesLineup = new();  // Who I have available in the Party

    public SerializableHashSet<QuestStep> CompletedSteps = new();
    public SerializableHashSet<Quest> DiscoveredQuests = new();
    public SerializableHashSet<BaseItem> DiscoveredItems = new();
    public SerializableDictionary<guid, HostileStats> HostileStats = new();

    public IEnumerable<HeroExtension> AllHeroes => PartyLineup.Concat(ReservesLineup);
    public TimeSpan DurationTotal => _stopwatch.Elapsed + _lastPlaytime;
    guid ISaved.UniqueConstID => Guid;

    TimeSpan _lastPlaytime;
    System.Diagnostics.Stopwatch _stopwatch = System.Diagnostics.Stopwatch.StartNew();

    void Awake()
    {
        if (Instance == null!)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning($"Destroyed {gameObject}, no two {nameof(GameManager)} can coexist");
            Destroy(this);
            return;
        }

        transform.parent = null;
        DontDestroyOnLoad(this.gameObject);

        SavingSystem.TryRestore<GameManager, SaveV3>(this);
        for (int i = 0; i < PartyLineup.Count; i++)
        {
            PartyLineup[i] = Instantiate(PartyLineup[i], transform);
            PartyLineup[i].name = PartyLineup[i].name[..^"(Clone)".Length]; // Remove the postfix unity inserts when instantiating
        }
        for (int i = 0; i < ReservesLineup.Count; i++)
        {
            ReservesLineup[i] = Instantiate(ReservesLineup[i], transform);
            ReservesLineup[i].name = ReservesLineup[i].name[..^"(Clone)".Length]; // Remove the postfix unity inserts when instantiating
        }

        foreach (var heroExtension in AllHeroes)
        {
            SavingSystem.TryRestore<HeroExtension, HeroExtension.SaveV2>(heroExtension);
        }
    }

    void Update()
    {
        var singleton = SingletonManager.Instance;
        singleton.HunterRank.Amount = (int)CharacterTemplate.GetAmountOfLevelForXP((uint)singleton.HunterExperience.Amount);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null!;

        SavingSystem.Unregister<GameManager, SaveV3>(this);

        foreach (var heroExtension in AllHeroes)
        {
            SavingSystem.Unregister<HeroExtension, HeroExtension.SaveV2>(heroExtension);
        }
    }

    /// <summary>
    /// A coroutine that won't stop once its object is disabled,
    /// <paramref name="destroyDependency"/> is to ensure the coroutine stops when the object is destroyed
    /// </summary>
    public void StartUndisablableCoroutine(UnityEngine.Object destroyDependency, IEnumerator coroutine)
    {
        StartCoroutine(CoroutineRunner());

        IEnumerator CoroutineRunner()
        {
            for (var e = coroutine; e.MoveNext() && destroyDependency;)
                yield return e.Current;
        }
    }

    [Serializable] public struct SaveV3 : ISaveHandler<GameManager>, ISaveDataVersioned<SaveV2>
    {
        public guid[] DiscoveredItems;
        public HostileStatPair[] HostileStats;
        public SaveV2 PreviousData;
        
        public uint Version => 3;

        public void Transfer(GameManager source, SavingSystem.Transfer transfer)
        {
            PreviousData.Transfer(source, transfer);
            if (transfer == SavingSystem.Transfer.PullFromSource)
            {
                DiscoveredItems = source.DiscoveredItems.Select(x => x.Guid).ToArray();
                HostileStats = source.HostileStats.Select(x => new HostileStatPair
                {
                    Hostile = x.Key,
                    Stats = x.Value,
                }).ToArray();
            }
            else
            {
                source.DiscoveredItems = new();
                foreach (var guid in DiscoveredItems)
                {
                    if (IdentifiableDatabase.TryGet(guid, out BaseItem? item))
                        source.DiscoveredItems.Add(item!);
                    else
                        Debug.LogWarning($"Could not find item {guid}");
                }
                source.HostileStats = new();
                foreach (var pair in HostileStats)
                {
                    if (IdentifiableDatabase.TryGet(pair.Hostile, out CharacterTemplate? item))
                        source.HostileStats.Add(pair.Hostile, pair.Stats);
                    else
                        Debug.LogWarning($"Could not find item {pair.Hostile}");
                }
            }
        }

        public void UpgradeFromPrevious(SaveV2 old)
        {
            PreviousData = old;
        }

        [Serializable]
        public struct HostileStatPair
        {
            public guid Hostile;
            public HostileStats Stats;
        }
    }

    [Serializable] public struct SaveV2 : ISaveDataVersioned<SaveV1>
    {
        public TimeSpan TimeSpan => TimeSpan.FromTicks(Ticks);
        public long Ticks;
        public guid[] Party, Reserve;
        public QuestStep[] CompletedSteps;
        public guid[] DiscoveredQuests;

        public uint Version => 2;

        public void Transfer(GameManager source, SavingSystem.Transfer transfer)
        {
            if (transfer == SavingSystem.Transfer.PullFromSource)
            {
                Party = source.PartyLineup.Select(x => x.Guid).ToArray();
                Reserve = source.ReservesLineup.Select(x => x.Guid).ToArray();
                Ticks = source.DurationTotal.Ticks;
                CompletedSteps = source.CompletedSteps.Select(x => new QuestStep { quest = x.Quest.Guid, step = x.Guid }).ToArray();
                DiscoveredQuests = source.DiscoveredQuests.Select(x => x.Guid).ToArray();
            }
            else
            {
                source.PartyLineup = new();
                foreach (guid guid in Party)
                    if (IdentifiableDatabase.TryGet(guid, out HeroExtension? hero))
                        source.PartyLineup.Add(hero!);

                source.ReservesLineup = new();
                foreach (guid guid in Reserve)
                    if (IdentifiableDatabase.TryGet(guid, out HeroExtension? hero))
                        source.ReservesLineup.Add(hero!);

                source._lastPlaytime = TimeSpan.FromTicks(Ticks);
                source._stopwatch.Restart();
                source.CompletedSteps = new();
                foreach (var questStep in CompletedSteps)
                {
                    if (IdentifiableDatabase.TryGet(questStep.quest, out Quest? quest))
                    {
                        if (quest!.Steps.FirstOrDefault(x => x.Guid == questStep.step) is { } step)
                            source.CompletedSteps.Add(step);
                        else
                            Debug.LogWarning($"Could not find step {questStep.step} in quest {quest}");
                    }
                    else
                        Debug.LogWarning($"Could not find quest {questStep.quest}");
                }
                source.DiscoveredQuests = new();
                foreach (var guid in DiscoveredQuests)
                {
                    if (IdentifiableDatabase.TryGet(guid, out Quest? quest))
                        source.DiscoveredQuests.Add(quest!);
                    else
                        Debug.LogWarning($"Could not find quest {guid}");
                }
            }
        }

        public void UpgradeFromPrevious(SaveV1 old)
        {
            Ticks = old.Ticks;
            Party = old.Party;
            Reserve = old.Reserve;
        }

        [Serializable]
        public struct QuestStep
        {
            public guid quest, step;
        }
    }

    [Serializable] public struct SaveV1 : ISaveData
    {
        public long Ticks;
        public guid[] Party, Reserve;

        public uint Version => 1;

        public void Transfer(GameManager source, SavingSystem.Transfer transfer)
        {
            if (transfer == SavingSystem.Transfer.PullFromSource)
            {
                Party = source.PartyLineup.Select(x => x.Guid).ToArray();
                Reserve = source.ReservesLineup.Select(x => x.Guid).ToArray();
                Ticks = source.DurationTotal.Ticks;
            }
            else
            {
                source.PartyLineup = new();
                foreach (guid guid in Party)
                    if (IdentifiableDatabase.TryGet(guid, out HeroExtension? hero))
                        source.PartyLineup.Add(hero!);

                source.ReservesLineup = new();
                foreach (guid guid in Reserve)
                    if (IdentifiableDatabase.TryGet(guid, out HeroExtension? hero))
                        source.ReservesLineup.Add(hero!);

                source._lastPlaytime = TimeSpan.FromTicks(Ticks);
                source._stopwatch.Restart();
            }
        }
    }

    static GameManager()
    {
        DomainReloadHelper.BeforeReload += helper => helper.GMInstance = Instance;
        DomainReloadHelper.AfterReload += helper => Instance = helper.GMInstance!;
    }
}

public partial class DomainReloadHelper
{
    public GameManager? GMInstance;
}
