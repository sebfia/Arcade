using System;
using System.Threading;
using Arcade.Run.Execution;

namespace Arcade.Run.Messages
{
    public sealed class FlowCancelledMessage : IRuntimeMessage
    {
        public readonly Guid CorrelationId;
        public readonly CancellationTokenSource CancellationTokenSource;

        public FlowCancelledMessage(RunId runId, Guid correlationId, CancellationTokenSource cancellationTokenSource, Guid? contextId = null)
        {
            CorrelationId = correlationId;
            CancellationTokenSource = cancellationTokenSource;
            RunId = runId;
            ContextId = contextId;
        }

        public RunId RunId { get; private set; }
        public Guid? ContextId { get; private set; }
    }
}