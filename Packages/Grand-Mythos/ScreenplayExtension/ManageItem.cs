using System;
using System.Collections.Generic;
using Screenplay;
using Screenplay.Nodes;
using Sirenix.OdinInspector;
using YNode;
using Action = Screenplay.Nodes.Action;

[Serializable, NodeWidth(NodeWidthAttribute.Default + 64)]
public class ManageItem : Action
{
    [HideLabel, HorizontalGroup(Width = 76)]
    public Operation Op = Operation.Give;
    [ValidateInput(nameof(ValidateCount), "Must be greater than 0!"), HideLabel, HorizontalGroup(Width = 32)]
    public uint Count = 1;
    [Required, HideLabel, HorizontalGroup]
    public BaseItem Item;

    bool ValidateCount(uint count) => count > 0;
        
    public override void CollectReferences(List<GenericSceneObjectReference> references)
    {
        
    }

    public override IEnumerable<Signal> Execute(IContext context)
    {
        switch (Op)
        {
            case Operation.Give:
                InventoryManager.Instance.AddToInventory(Item, Count);
                break;
            case Operation.Remove:
                InventoryManager.Instance.Remove(Item, Count);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        yield return Signal.BreakInto(Next);
    }

    public override void FastForward(IContext context)
    {
        // No need to fast-forward this event as the inventory is already saved 
    }

    public override void SetupPreview(IPreviewer previewer, bool fastForwarded)
    {
        
    }

    public enum Operation
    {
        Give,
        Remove
    }
}