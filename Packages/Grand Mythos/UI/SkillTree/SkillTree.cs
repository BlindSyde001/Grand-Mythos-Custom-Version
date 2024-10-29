using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class SkillTree : MonoBehaviour
{
    [Required] public UnlockNode Root;
    [ReadOnly] public HeroExtension SelectedHero;
    [ReadOnly, SerializeField] SerializableDictionary<UnlockNode, guid> _nodeToGuid = new();
    [ReadOnly, SerializeField] SerializableHashSet<UnlockNode> _reachableNodes = new();

    void Start()
    {
        if (Root == null)
            Debug.LogError($"You must set a node as the {nameof(Root)} on {gameObject}'s {nameof(SkillTree)} otherwise nodes cannot be unlocked by the user");

        foreach (var (node, guid) in _nodeToGuid)
        {
            node.Button.onClick.AddListener(() => TryUnlock(node));
            if (SelectedHero.UnlockedTreeNodes.TryGetValue(guid, out var count))
            {
                for (int i = 0; i < node.Unlocks.Length; i++)
                    node.Subnodes[i].OnLock.Invoke();
                for (int i = 0; i < count && i < node.Unlocks.Length; i++)
                    node.Subnodes[node.Unlocks.Length - 1 - i].OnUnlock.Invoke();
            }
        }
        _reachableNodes.Clear();
        UpdateReachableNodes();
        foreach (var (node, guid) in _nodeToGuid)
            if (_reachableNodes.Contains(node) == false)
                node.OnUnreachable?.Invoke();
    }

    void UpdateReachableNodes()
    {
        if (_reachableNodes.Add(Root))
            Root.OnReachable?.Invoke();

        foreach (var (node, guid) in _nodeToGuid)
        {
            if (SelectedHero.UnlockedTreeNodes.ContainsKey(guid) == false)
                continue;

            if (_reachableNodes.Add(node))
                node.OnReachable?.Invoke();

            foreach (var linked in node.LinkedTo)
                if (_reachableNodes.Add(linked))
                    linked.OnReachable?.Invoke();
        }
    }

    public Dictionary<guid, UnlockNode> GetNodes()
    {
        Dictionary<guid, UnlockNode> nodes = new Dictionary<guid, UnlockNode>();
        foreach (var node in _nodeToGuid)
            nodes.Add(node.Value, node.Key);

        return nodes;
    }

    public Dictionary<UnlockNode, guid>.Enumerator NodesEnum() => _nodeToGuid.GetEnumerator();

    public void EnsureRegistered(UnlockNode nodeDrawer)
    {
        if (_nodeToGuid.ContainsKey(nodeDrawer) == false)
            _nodeToGuid.Add(nodeDrawer, System.Guid.NewGuid());

        foreach (var keyValuePair in _nodeToGuid.Where(x => x.Key == null).ToArray())
            _nodeToGuid.Remove(keyValuePair.Key);
    }

    public void TryUnlock(UnlockNode node)
    {
        if (SelectedHero.SkillPointsAllocated >= SelectedHero.SkillPointsMax)
            return;

        if (node != Root && _reachableNodes.Contains(node) == false)
            return;

        var guid = _nodeToGuid[node];
        var unlockedNodes = SelectedHero.UnlockedTreeNodes;
        if (unlockedNodes.TryGetValue(guid, out var count) && count >= node.Unlocks.Length)
            return;

        unlockedNodes[guid] = count + 1;
        node.Unlocks[count].OnUnlock(SelectedHero);
        node.Subnodes[node.Unlocks.Length - 1 - count].OnUnlock.Invoke();
        UpdateReachableNodes();
    }
}