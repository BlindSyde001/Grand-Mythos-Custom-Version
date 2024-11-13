using System;
using System.Collections.Generic;
using Screenplay;
using Screenplay.Nodes;
using Sirenix.OdinInspector;

namespace Quests
{
    public class IsCurrentStep : ScreenplayNode, IPrerequisite
    {
        [Required, HideLabel] public QuestStep Step;
        
        public override void CollectReferences(List<GenericSceneObjectReference> references) { }

        public bool TestPrerequisite(HashSet<IPrerequisite> visited)
        {
            if (Step.Quest.Discovered == false)
                return false;
            
            if (Step.Completed)
                return false;

            var i = Array.IndexOf(Step.Quest.Steps, Step);
            return i == 0 || Step.Quest.Steps[i-1].Completed;
        }
    }
}