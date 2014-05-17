using System;

namespace Arcade.Run.Execution.Messages
{
    public sealed class ExceptionRunVectorExecutedMessage : RunVectorExecutedMessageBase
    {
        public readonly Exception Exception;
        public readonly object CausativeInput;

        public ExceptionRunVectorExecutedMessage(RunId runId, Guid correlationId, Exception exception, object causativeInput)
            : base(runId, correlationId)
        {
            Exception = exception;
            CausativeInput = causativeInput;
        }
    }
    
}