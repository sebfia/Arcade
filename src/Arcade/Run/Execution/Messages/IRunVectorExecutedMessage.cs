using System;
using Arcade.Run.RunVectors;

namespace Arcade.Run.Execution.Messages
{
    /// <summary>
    /// Marker interface for messages that are created and passed around when a single <see cref="IRunVector"/> has been executed. 
    /// </summary>
    public interface IRunVectorExecutedMessage
    {
        Guid CorrelationId { get; }
    }
    
}