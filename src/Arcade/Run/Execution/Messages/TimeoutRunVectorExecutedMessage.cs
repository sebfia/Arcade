using System;

namespace Arcade.Run.Execution.Messages
{
    public sealed class TimeoutRunVectorExecutedMessage : RunVectorExecutedMessageBase
    {
        public readonly TimeSpan Timeout;
        public readonly object CausativeInput;

        public TimeoutRunVectorExecutedMessage(RunId runId, Guid correlationId, TimeSpan timeout, object causativeInput)
            : base(runId, correlationId)
        {
            Timeout = timeout;
            CausativeInput = causativeInput;
        }
    }
    
}