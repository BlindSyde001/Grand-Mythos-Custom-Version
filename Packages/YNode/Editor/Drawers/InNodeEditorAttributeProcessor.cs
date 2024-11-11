#if UNITY_EDITOR && ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace YNode.Editor
{
    internal class OdinNodeInGraphAttributeProcessor : OdinAttributeProcessor<NodeEditor>
    {
        public override bool CanProcessSelfAttributes(InspectorProperty property) => false;

        public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member)
        {
            if (!GraphWindow.InNodeEditor)
                return false;

            if (member.MemberType == MemberTypes.Field)
            {
                switch (member.Name)
                {
                    case "_reroutePoints":
                    case nameof(NodeEditor.Graph):
                        return true;
                }
            }

            return false;
        }

        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member,
            List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case "_reroutePoints":
                case nameof(NodeEditor.Graph):
                    attributes.Add(new HideInInspector());
                    break;
            }
        }
    }
}
#endif