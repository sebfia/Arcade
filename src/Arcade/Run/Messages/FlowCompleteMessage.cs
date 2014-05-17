using System;
using System.Threading;
using Arcade.Run.Execution;

namespace Arcade.Run.Messages
{
    public sealed class FlowCompleteMessage : IRuntimeMessage
    {
        public RunId RunId { get; private set; }
        public Guid? ContextId { get; private set; }
        public readonly Result Result;
        public readonly CancellationTokenSource CancellationTokenSource;

        public FlowCompleteMessage(RunId runId, Result result, CancellationTokenSource cancellationTokenSource, Guid? contextId = null)
        {
            RunId = runId;
            Result = result;
            CancellationTokenSource = cancellationTokenSource;
            ContextId = contextId;
        }
    }
}