using System.Collections.Generic;

namespace Screenplay.Nodes
{
    /// <summary>
    /// A node that can affect the game state in some way
    /// </summary>
    public interface IAction : IPrerequisite, IPreviewable
    {
        /// <summary>
        /// The next action which will be played out, none if this is the last one,
        /// a single one for most <see cref="IAction"/>,
        /// multiple in case of branching through <see cref="Choice"/> for example.
        /// </summary>
        /// <remarks>
        /// Used to traverse the node tree, providing insight about nodes that are reachable
        /// </remarks>
        IEnumerable<IAction> Followup();

        /// <summary>
        /// Run this particular action.
        /// <para/>yield <see cref="Signal.NextFrame"/> to pause execution until the next frame,
        /// <para/>yield <see cref="Signal.BreakInto"/> to stop execution and enqueue the provided action as the next one to be executed.
        /// </summary>
        IEnumerable<Signal> Execute(IContext context);

        /// <summary>
        /// Execute this action to completion in a single call, applying any side effects the <see cref="Execute"/> would have introduced if it ran in its stead
        /// </summary>
        /// <remarks>
        /// Used when loading game saves, calling this over each node that have been visited in the save.
        /// </remarks>
        void FastForward(IContext context);
    }
}
