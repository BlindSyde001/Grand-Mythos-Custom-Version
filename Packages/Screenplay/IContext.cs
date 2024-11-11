using System.Collections.Generic;
using Screenplay.Nodes;

namespace Screenplay
{
    /// <summary>
    /// Working data when executing a <see cref="ScreenplayGraph"/> shared for the whole run
    /// </summary>
    public interface IContext
    {
        ScreenplayGraph Source { get; }

        /// <summary> Nodes visited </summary>
        HashSet<IPrerequisite> Visited { get; }

        /// <summary>
        /// Creates the dialog component if it doesn't exist yet and return it
        /// </summary>
        Component.UIBase? GetDialogUI();

        /// <summary>
        /// Run this enumerable asynchronously to the <see cref="ScreenplayGraph"/>, may be interrupted when <see cref="StopAsynchronous"/> is called with the same key.
        /// </summary>
        void RunAsynchronously(object key, IEnumerable<Signal> runner);

        /// <summary>
        /// Stop any enumerable running with the same key previously scheduled through <see cref="RunAsynchronously"/>
        /// </summary>
        bool StopAsynchronous(object key);
    }
}
