using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YNode;

namespace Screenplay.Nodes.Logic
{
    [NodeWidth(100)]
    public class And : ScreenplayNode, IPrerequisite
    {
        [Input(Stroke = NoodleStroke.Dashed), SerializeReference, Required]
        public IPrerequisite A = null!, B = null!;

        public bool TestPrerequisite(HashSet<IPrerequisite> visited) => A.TestPrerequisite(visited) && B.TestPrerequisite(visited);

        public override void CollectReferences(List<GenericSceneObjectReference> references)
        {
            A?.CollectReferences(references);
            B?.CollectReferences(references);
        }
    }
}
