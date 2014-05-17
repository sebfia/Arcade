using System;
using Arcade.Run.Execution.Events;
using Arcade.Run.Execution.Messages;

namespace Arcade.Run.Execution.Post
{
    public sealed class WaitOnPortRunVectorExecutedMessageStrategy : IRunVectorExecutedMessageStrategy
    {
        public bool CanTreatRunVectorExecutedMessage(IRunVectorExecutedMessage runVectorExecutedMessage)
        {
            return runVectorExecutedMessage is WaitOnPortRunVectorExecutedMessage;
        }

        public void TreatRunVectorExecutedMessage(IRunVectorExecutedMessage runVectorExecutedMessage,
                                                  IFlowStackOrchestrator orchestrator)
        {
            var message = runVectorExecutedMessage as WaitOnPortRunVectorExecutedMessage;

            if (message == null) throw new InvalidOperationException("Unable to treat this type of RunVectorExecutedMessage. Check CanTreatRunVectorExecutedMessage first!");

            orchestrator.PropagateEvent(new WaitOnPortEvent(message.RunId, message.NextCorrelationId, message.PortName, message.Result));
        }
    }
}