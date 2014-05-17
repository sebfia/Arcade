using System;
using Arcade.Run.Execution;

namespace Arcade.Run.Messages
{
    /// <summary>
    /// Currently only a marker interface for messages that are being passed around within the runtime engine.
    /// </summary>
    public interface IRuntimeMessage
    {
        RunId RunId { get; }
        Guid? ContextId { get; }
    }
}