using System;
using Arcade.Run.Execution;

namespace Arcade.Run.RunVectors
{
    /// <summary>
    /// Embeds the smallest flow unit in a way that can be run universally in a <see cref="IRunFlowStack"/> instance. 
    /// </summary>
    public interface IRunVector
    {
        Guid CorrelationId { get; }
        TimeSpan Timeout { get; }
        Guid NextCorrelationId { get; }
    }
}