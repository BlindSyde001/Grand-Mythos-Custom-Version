using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace YNode.Editor
{
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public class SkipPolymorphicFieldDrawer : OdinAttributeDrawer<SkipPolymorphicFieldAttribute>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var chain = Property.GetActiveDrawerChain();
            chain.MoveNext();
            chain.MoveNext();
            CallNextDrawer(label);
        }
    }
}
