using System;
using Arcade.Run.Execution.Events;
using Arcade.Run.Execution.Messages;

namespace Arcade.Run.Execution.Post
{
    public sealed class FinalRunVectorExecutedMessageStrategy : IRunVectorExecutedMessageStrategy
    {
        public bool CanTreatRunVectorExecutedMessage(IRunVectorExecutedMessage runVectorExecutedMessage)
        {
            return runVectorExecutedMessage is FinalRunVectorExecutedMessage;
        }        
        
        public void TreatRunVectorExecutedMessage(IRunVectorExecutedMessage runVectorExecutedMessage, IFlowStackOrchestrator orchestrator)
        {
            var message = runVectorExecutedMessage as FinalRunVectorExecutedMessage;
            
            if(message == null) throw new InvalidOperationException("Unable to treat this type of RunVectorExecutedMessage. Check CanTreatRunVectorExecutedMessage first!");

            orchestrator.PropagateEvent(new FlowFinishedEvent(message.RunId, message.Result));
        }
    }

}