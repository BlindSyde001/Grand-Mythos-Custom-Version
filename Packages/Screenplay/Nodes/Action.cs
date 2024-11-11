using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YNode;

namespace Screenplay.Nodes
{
    [Serializable]
    public abstract class Action : ScreenplayNode, IAction
    {
        [Output, SerializeReference, HideLabel, Tooltip("What would run right after this is done running")]
        public IAction? Next;

        public bool TestPrerequisite(HashSet<IPrerequisite> visited) => visited.Contains(this);

        public IEnumerable<IAction> Followup()
        {
            if (Next != null)
                yield return Next;
        }

        public abstract IEnumerable<Signal> Execute(IContext context);
        public abstract void FastForward(IContext context);

        public abstract void SetupPreview(IPreviewer previewer, bool fastForwarded);
    }
}
