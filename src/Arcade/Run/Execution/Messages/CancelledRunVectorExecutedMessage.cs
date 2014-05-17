using System;

namespace Arcade.Run.Execution.Messages
{
    public sealed class CancelledRunVectorExecutedMessage : RunVectorExecutedMessageBase
    {
        public CancelledRunVectorExecutedMessage(RunId runId, Guid correlationId) 
            : base(runId, correlationId)
        {

        }
    }
}