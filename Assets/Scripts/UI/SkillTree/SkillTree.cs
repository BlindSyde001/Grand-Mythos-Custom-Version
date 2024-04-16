using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class SkillTree : MonoBehaviour
{
    [ReadOnly] public HeroExtension SelectedHero;
    [ReadOnly, SerializeField] SerializableDictionary<UnlockNode, guid> _nodeToGuid = new();
    [ReadOnly, SerializeField] SerializableHashSet<UnlockNode> _reachableNodes = new();

    void Start()
    {
        _reachableNodes.Clear();
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
        foreach (var (node, _) in _nodeToGuid)
        {
            if (_reachableNodes.Contains(node))
                continue;

            bool reachable = node.Requirements.Length == 0;
            foreach (var requirement in node.Requirements)
                if (SelectedHero.UnlockedTreeNodes.Contains(_nodeToGuid[requirement]))
                    reachable = true;

            if (reachable)
            {
                _reachableNodes.Add(node);
                node.OnReachable?.Invoke();
            }
        }
    }

    public Dictionary<guid, UnlockNode> GetNodes()
    {
        Dictionary<guid, UnlockNode> nodes = new Dictionary<guid, UnlockNode>();
        foreach (var node in _nodeToGuid)
            nodes.Add(node.Value, node.Key);

        return nodes;
    }

    public void EnsureRegistered(UnlockNode nodeDrawer)
    {
        if (_nodeToGuid.ContainsKey(nodeDrawer) == false)
            _nodeToGuid.Add(nodeDrawer, System.Guid.NewGuid());

        foreach (var keyValuePair in _nodeToGuid.Where(x => x.Key == null).ToArray())
            _nodeToGuid.Remove(keyValuePair.Key);
    }

    public void TryUnlock(UnlockNode nodeDrawer)
    {
        var unlockedNodes = SelectedHero.UnlockedTreeNodes;
        if (unlockedNodes.Count >= SelectedHero.SkillPointsTotal)
            return;

        if (nodeDrawer.Requirements.Length != 0 && nodeDrawer.Requirements.Any(requirement => unlockedNodes.Contains(_nodeToGuid[requirement])) == false)
            return;

        var guid = _nodeToGuid[nodeDrawer];
        if (unlockedNodes.Add(guid) == false)
            return;

        nodeDrawer.Unlock.OnUnlock(SelectedHero, guid);
        nodeDrawer.OnUnlock?.Invoke();
        UpdateReachableNodes();
    }
}