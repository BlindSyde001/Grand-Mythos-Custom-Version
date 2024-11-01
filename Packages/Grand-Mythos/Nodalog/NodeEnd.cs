#nullable enable

using UnityEngine;
using Unity.VisualScripting;

namespace Nodalog
{
    [UnitCategory("Nodalog")]
    [UnitTitle("End")]
    [TypeIcon(typeof(DialogUIComponent))]
    public class NodeEnd : Unit
    {
        [DoNotSerialize, PortLabelHidden]
        public ControlInput? Enter { get; private set; }

        [DoNotSerialize, PortLabelHidden]
        public ControlOutput? Exit { get; private set; }

        protected override void Definition()
        {
            Exit = ControlOutput(nameof(Exit));
            Enter = ControlInput(nameof(Enter), flow =>
            {
                if (NodeStart.Starts.Remove(flow, out var data) && data.fromPrefab)
                {
                    data.ui.EndDialogPresentation();
                    Object.Destroy(data.ui.gameObject);
                }

                return Exit;
            });
            Succession(Enter, Exit);
        }
    }
}