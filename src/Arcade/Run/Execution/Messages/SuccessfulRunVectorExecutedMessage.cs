using System;

namespace Arcade.Run.Execution.Messages
{
    public sealed class SuccessfulRunVectorExecutedMessage : RunVectorExecutedMessageBase
    {
        public readonly Result Result;
        public readonly Guid NextCorrelationId;

        public SuccessfulRunVectorExecutedMessage(RunId runId, Guid correlationId, Guid nextCorrelationId, Result result)
            : base(runId, correlationId)
        {
            Result = result;
            NextCorrelationId = nextCorrelationId;
        }
    }
    
}