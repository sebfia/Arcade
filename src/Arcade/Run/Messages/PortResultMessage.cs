using System;
using System.Threading;
using Arcade.Run.Execution;

namespace Arcade.Run.Messages
{
    public sealed class PortResultMessage : IRuntimeMessage
    {
        public readonly Guid CorrelationIdToContinueWith;
        public readonly Result Result;
        public readonly CancellationTokenSource CancellationTokenSource;

        public PortResultMessage(RunId runId, Guid correlationIdToContinueWith, Result result, CancellationTokenSource cancellationTokenSource, Guid? contextId = null)
        {
            RunId = runId;
            CorrelationIdToContinueWith = correlationIdToContinueWith;
            Result = result;
            CancellationTokenSource = cancellationTokenSource;
            ContextId = contextId;
        }

        public RunId RunId { get; private set; }
        public Guid? ContextId { get; private set; }
    }
}