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
            (SelectedHero.UnlockedTreeNodes.Contains(guid) ? node.OnUnlock : node.OnLock)?.Invoke();
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
            if (SelectedHero.UnlockedTreeNodes.Contains(guid) == false)
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
        var unlockedNodes = SelectedHero.UnlockedTreeNodes;
        if (unlockedNodes.Count >= SelectedHero.SkillPointsTotal)
            return;

        if (node != Root && _reachableNodes.Contains(node) == false)
            return;

        var guid = _nodeToGuid[node];
        if (unlockedNodes.Add(guid) == false)
            return;

        node.Unlock.OnUnlock(SelectedHero, guid);
        node.OnUnlock?.Invoke();
        UpdateReachableNodes();
    }
}