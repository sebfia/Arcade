using System;
using Arcade.Run.Execution.Events;
using Arcade.Run.Execution.Messages;

namespace Arcade.Run.Execution.Post
{
    public sealed class ContinueWithNamedFlowExecutedMessageStrategy : IRunVectorExecutedMessageStrategy
    {
        public bool CanTreatRunVectorExecutedMessage(IRunVectorExecutedMessage runVectorExecutedMessage)
        {
            return runVectorExecutedMessage is ContinueWithNamedFlowRunVectorExecutedMessage;
        }

        public void TreatRunVectorExecutedMessage(IRunVectorExecutedMessage runVectorExecutedMessage, IFlowStackOrchestrator orchestrator)
        {
            var message = runVectorExecutedMessage as ContinueWithNamedFlowRunVectorExecutedMessage;

            if(message == null) throw new InvalidOperationException("Unable to treat this kind of IRunVectorExecutedMessage");

            orchestrator.PropagateEvent(new InitializeChildFlowEvent(message.RunId, message.FlowName, message.NextCorrelationId, message.Result));
        }
    }
}