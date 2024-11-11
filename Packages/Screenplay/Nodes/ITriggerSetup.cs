using System.Diagnostics.CodeAnalysis;
using Screenplay.Nodes.Triggers;

namespace Screenplay.Nodes
{
    /// <summary>
    /// Takes care of setting up new triggers in the game world.
    /// For example, adding an interaction point in the world triggering the event linked.
    /// </summary>
    public interface ITriggerSetup : IReferenceContainer
    {
        /// <summary>
        /// Tries to create a new trigger, when successful the trigger will call <paramref name="onTriggered"/> when triggered
        /// </summary>
        bool TryCreateTrigger(System.Action onTriggered, [MaybeNullWhen(false)] out ITrigger trigger);
    }
}
