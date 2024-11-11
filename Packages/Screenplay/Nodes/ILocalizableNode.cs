using System.Collections.Generic;
using YNode;

namespace Screenplay.Nodes
{
    /// <summary>
    /// A node that contain localizable data
    /// </summary>
    public interface ILocalizableNode : INodeValue
    {
        /// <summary>
        /// Return all localizable text instance
        /// </summary>
        /// <remarks>
        /// Used to remap the content to the correct locale
        /// </remarks>
        public IEnumerable<LocalizableText> GetTextInstances();
    }
}
