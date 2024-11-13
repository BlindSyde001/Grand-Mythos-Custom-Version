using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YNode;

namespace Screenplay.Nodes
{
    [Serializable]
    public class Branch : ScreenplayNode, IAction
    {
        [Output, SerializeReference, Tooltip("What would run when Prerequisite is true")]
        public IAction? True;

        [Output, SerializeReference, Tooltip("What would run when Prerequisite is false")]
        public IAction? False;

        [Input(Stroke = NoodleStroke.Dashed), Required, SerializeReference, LabelWidth(20), HorizontalGroup(width:90), Tooltip("Select which action should be taken next")]
        public IPrerequisite Prerequisite;

        public bool TestPrerequisite(HashSet<IPrerequisite> visited) => visited.Contains(this);

        public IEnumerable<IAction> Followup()
        {
            if (True != null)
                yield return True;
            if (False != null)
                yield return False;
        }

        public IEnumerable<Signal> Execute(IContext context)
        {
            if (Prerequisite.TestPrerequisite(context.Visited))
                yield return Signal.BreakInto(True);
            else
                yield return Signal.BreakInto(False);
        }

        public void FastForward(IContext context) { }

        public void SetupPreview(IPreviewer previewer, bool fastForwarded)
        {
            if (fastForwarded == false)
                previewer.PlaySafeAction(this);
        }

        public override void CollectReferences(List<GenericSceneObjectReference> references) { }
    }
}
