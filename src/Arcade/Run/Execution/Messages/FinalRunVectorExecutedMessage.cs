using System;

namespace Arcade.Run.Execution.Messages
{
    public sealed class FinalRunVectorExecutedMessage : RunVectorExecutedMessageBase
    {
        public readonly Result Result;

        public FinalRunVectorExecutedMessage(RunId runId, Guid correlationId, Result result)
            : base(runId, correlationId)
        {
            Result = result;
        }
    }
    
}