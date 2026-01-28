using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Screenplay;
using Screenplay.Nodes;
using Sirenix.OdinInspector;
using YNode;

[Serializable, NodeVisuals(Width = NodeVisualsAttribute.DefaultWidth  + 64)]
public class ManageItem : ExecutableLinear
{
    [HideLabel, HorizontalGroup(Width = 76)]
    public Operation Op = Operation.Give;
    [ValidateInput(nameof(ValidateCount), "Must be greater than 0!"), HideLabel, HorizontalGroup(Width = 32)]
    public uint Count = 1;
    [HideLabel, HorizontalGroup]
    public required BaseItem Item;

    bool ValidateCount(uint count) => count > 0;
        
    public override void CollectReferences(ReferenceCollector references)
    {
        
    }

    protected override UniTask LinearExecution(IEventContext context, CancellationToken cancellation)
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

        return UniTask.CompletedTask;
    }

    public override UniTask Persistence(IEventContext context, CancellationToken cancellationToken) => UniTask.CompletedTask;

    public override void SetupPreview(IPreviewer previewer, bool fastForwarded)
    {
        
    }

    public enum Operation
    {
        Give,
        Remove
    }
}