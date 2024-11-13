using System;
using System.Collections.Generic;
using Screenplay;
using Screenplay.Nodes;
using Sirenix.OdinInspector;

namespace Quests
{
    [Serializable]
    public class QuestDiscovered : ScreenplayNode, IPrerequisite
    {
        [Required, HideLabel] public Quest Quest;
        
        public override void CollectReferences(List<GenericSceneObjectReference> references) { }

        public bool TestPrerequisite(HashSet<IPrerequisite> visited) => Quest.Completed;
    }
}