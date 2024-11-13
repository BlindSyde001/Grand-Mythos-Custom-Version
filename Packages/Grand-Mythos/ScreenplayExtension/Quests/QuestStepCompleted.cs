using System;
using System.Collections.Generic;
using Screenplay;
using Screenplay.Nodes;
using Sirenix.OdinInspector;

namespace Quests
{
    [Serializable]
    public class QuestStepCompleted : ScreenplayNode, IPrerequisite
    {
        [Required, HideLabel] public QuestStep Step;
        
        public override void CollectReferences(List<GenericSceneObjectReference> references) { }

        public bool TestPrerequisite(HashSet<IPrerequisite> visited) => Step.Completed;
    }
}