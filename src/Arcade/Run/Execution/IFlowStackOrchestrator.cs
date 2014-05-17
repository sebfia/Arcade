using System;
using Arcade.Run.Execution.Events;
using Arcade.Run.RunVectors;

namespace Arcade.Run.Execution
{
    /// <summary>
    /// Orchestrates how to continue with the next <see cref="IRunVector"/>. 
    /// </summary>
    public interface IFlowStackOrchestrator
    {
        void RunAfterSuccessfulTerminationOfPrevious(Guid nextCorrelationId, Result previousResult);
        void PropagateEvent(IRunFlowStackEvent runFlowStackEvent);
    }
}