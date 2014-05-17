using System;
using Arcade.Run.Execution.Events;
using Arcade.Run.Execution.Messages;

namespace Arcade.Run.Execution.Post
{
    public sealed class GatherRunVectorExecutedMessageStrategy : IRunVectorExecutedMessageStrategy
    {
        public bool CanTreatRunVectorExecutedMessage(IRunVectorExecutedMessage runVectorExecutedMessage)
        {
            return runVectorExecutedMessage is GatherRunVectorExecutedMessage;
        }

        public void TreatRunVectorExecutedMessage(IRunVectorExecutedMessage runVectorExecutedMessage,
                                                  IFlowStackOrchestrator orchestrator)
        {
            var message = runVectorExecutedMessage as GatherRunVectorExecutedMessage;

            if (message == null) throw new InvalidOperationException("Unable to treat this type of RunVectorExecutedMessage. Check CanTreatRunVectorExecutedMessage first!");

            orchestrator.PropagateEvent(new GatherFlowResultEvent(message.RunId, message.Result));
        }
    }
}