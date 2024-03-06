using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour, ISaved<GameManager, GameManager.SaveV1>
{
    public static readonly guid Guid = new("bb05002e-f0d5-4936-a986-c47a045e58d8");

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        transform.parent = null;
        SavingSystem.TryRestore<GameManager, SaveV1>(this);
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
    }

    void OnDestroy()
    {
        SavingSystem.StoreAndUnregister<GameManager, SaveV1>(this);
    }

    [FormerlySerializedAs("_PartyLineup"), BoxGroup("PARTY DATA")]
    public List<HeroExtension> PartyLineup;  // Who I've selected to be fighting
    [FormerlySerializedAs("_ReservesLineup"), BoxGroup("PARTY DATA")]
    public List<HeroExtension> ReservesLineup;  // Who I have available in the Party

    guid ISaved.UniqueConstID => Guid;

    [Serializable] public struct SaveV1 : ISaveHandler<GameManager>
    {
        public guid[] Party, Reserve;

        public uint Version => 1;

        public void Transfer(GameManager source, SavingSystem.Transfer transfer)
        {
            if (transfer == SavingSystem.Transfer.PullFromSource)
            {
                Party = source.PartyLineup.Select(x => x.Guid).ToArray();
                Reserve = source.ReservesLineup.Select(x => x.Guid).ToArray();
            }
            else
            {
                source.PartyLineup = new();
                foreach (guid guid in Party)
                    if (PlayableCharacters.TryGet(guid, out var hero))
                        source.PartyLineup.Add(hero);

                source.ReservesLineup = new();
                foreach (guid guid in Reserve)
                    if (PlayableCharacters.TryGet(guid, out var hero))
                        source.ReservesLineup.Add(hero);
            }
        }
    }

    static GameManager()
    {
        DomainReloadHelper.BeforeReload += helper => helper.GMInstance = Instance;
        DomainReloadHelper.AfterReload += helper => Instance = helper.GMInstance;
    }
}

public partial class DomainReloadHelper
{
    public GameManager GMInstance;
}
