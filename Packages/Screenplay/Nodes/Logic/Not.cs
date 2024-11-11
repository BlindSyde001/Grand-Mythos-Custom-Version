using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YNode;

namespace Screenplay.Nodes.Logic
{
    [NodeWidth(100)]
    public class Not : ScreenplayNode, IPrerequisite
    {
        [Input(Stroke = NoodleStroke.Dashed), SerializeReference, Required]
        public IPrerequisite A = null!;

        public bool TestPrerequisite(HashSet<IPrerequisite> visited) => A.TestPrerequisite(visited) == false;

        public override void CollectReferences(List<GenericSceneObjectReference> references)
        {
            A?.CollectReferences(references);
        }
    }
}
