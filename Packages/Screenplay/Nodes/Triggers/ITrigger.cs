using System;

namespace Screenplay.Nodes.Triggers
{
    /// <summary>
    /// Temporary trigger created through <see cref="ITriggerSetup.TryCreateTrigger"/>,
    /// may be disposed if the state of the <see cref="ScreenplayGraph"/> requires it.
    /// </summary>
    public interface ITrigger : IDisposable { }
}
