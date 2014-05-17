using System;
using Arcade.Run.Execution.Events;
using Arcade.Run.Execution.Messages;

namespace Arcade.Run.Execution.Post
{
    public sealed class CancelledRunVectorExecutedMessageStrategy : IRunVectorExecutedMessageStrategy
    {
        public bool CanTreatRunVectorExecutedMessage(IRunVectorExecutedMessage runVectorExecutedMessage)
        {
            return runVectorExecutedMessage is CancelledRunVectorExecutedMessage;
        }

        public void TreatRunVectorExecutedMessage(IRunVectorExecutedMessage runVectorExecutedMessage, IFlowStackOrchestrator orchestrator)
        {
            var message = runVectorExecutedMessage as CancelledRunVectorExecutedMessage;

            if (message == null) throw new InvalidOperationException("Unable to treat this type of RunVectorExecutedMessage. Check CanTreatRunVectorExecutedMessage first!");

            orchestrator.PropagateEvent(new FlowCancelledEvent(message.RunId, message.CorrelationId));
        }
    }
}