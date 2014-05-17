using System;
using System.Threading;
using Arcade.Run.Execution;

namespace Arcade.Run.Messages
{
    public sealed class FlowFailedMessage : IRuntimeMessage
    {
        public readonly Exception Exception;
        public readonly object CausativeInput;
        public readonly Guid CorrelationId;
        public readonly CancellationTokenSource CancellationTokenSource;

        public FlowFailedMessage(RunId runId, Exception exception, object causativeInput, Guid correlationId, CancellationTokenSource cancellationTokenSource, Guid? contextId = null)
        {
            Exception = exception;
            CausativeInput = causativeInput;
            CorrelationId = correlationId;
            CancellationTokenSource = cancellationTokenSource;
            RunId = runId;
            ContextId = contextId;
        }

        public RunId RunId { get; private set; }
        public Guid? ContextId { get; private set; }
    }
}