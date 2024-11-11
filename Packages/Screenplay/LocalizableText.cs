using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Screenplay
{
    /// <summary>
    /// <see cref="Content"/> will be replaced with the corresponding localized text through <see cref="ScreenplayGraph.GetLocalizableText"/>
    /// </summary>
    [Serializable]
    public class LocalizableText
    {
        [Text, HideLabel]
        public string Content = "";
        [HideInInspector, SerializeField]
        private guid _guid = guid.New();

        public guid Guid => _guid;

        public void ForceRegenerateGuid() => _guid = guid.New();
        public LocalizableText() { }
        public LocalizableText(string content) => Content = content;
    }
}
