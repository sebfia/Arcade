using Arcade.Run.Execution.Messages;

namespace Arcade.Run.Execution.Post
{
    /// <summary>
    /// Abstraction for the strategy to use for a specific <see cref="IRunVectorExecutedMessage"/> instance.
    /// </summary>
    public interface IRunVectorExecutedMessageStrategy
    {
        bool CanTreatRunVectorExecutedMessage(IRunVectorExecutedMessage runVectorExecutedMessage);
        void TreatRunVectorExecutedMessage(IRunVectorExecutedMessage runVectorExecutedMessage, IFlowStackOrchestrator orchestrator);
    }
    
}