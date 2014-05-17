using System;
using Arcade.Run.Execution.Events;
using Arcade.Run.Execution.Messages;

namespace Arcade.Run.Execution.Post
{
    public sealed class TriggerRunVectorExecutedMessageStrategy : IRunVectorExecutedMessageStrategy
    {
        public bool CanTreatRunVectorExecutedMessage(IRunVectorExecutedMessage runVectorExecutedMessage)
        {
            return runVectorExecutedMessage is TriggerRunVectorExecutedMessage;
        }

        public void TreatRunVectorExecutedMessage(IRunVectorExecutedMessage runVectorExecutedMessage,
                                                  IFlowStackOrchestrator orchestrator)
        {
            var message = runVectorExecutedMessage as TriggerRunVectorExecutedMessage;

            if (message == null) throw new InvalidOperationException("Unable to treat this type of RunVectorExecutedMessage. Check CanTreatRunVectorExecutedMessage first!");

            orchestrator.PropagateEvent(new TriggerEvent(message.RunId, message.TriggerName, message.SelectedResult));
            orchestrator.RunAfterSuccessfulTerminationOfPrevious(message.NextCorrelationId, message.Result);
        }
    }
}