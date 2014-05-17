using System;
using System.Threading;
using Arcade.Run.Execution;

namespace Arcade.Run.Messages
{
    public abstract class ContinueFlowMessage : IRuntimeMessage
    {
        public readonly Guid CorrelationIdToContinueFrom;
        public readonly CancellationTokenSource CancellationTokenSource;

        protected ContinueFlowMessage(RunId runId, Guid correlationIdToContinueFrom, CancellationTokenSource cancellationTokenSource, Guid? contextId)
        {
            RunId = runId;
            CorrelationIdToContinueFrom = correlationIdToContinueFrom;
            CancellationTokenSource = cancellationTokenSource;
            ContextId = contextId;
        }

        public RunId RunId { get; private set; }
        public Guid? ContextId { get; private set; }
    }

    public sealed class ContinueFailedFlowMessage : ContinueFlowMessage
    {
        public readonly object CausativeInput;
        public readonly Exception Exception;

        public ContinueFailedFlowMessage(RunId runId, Exception exception, object causativeInput, Guid correlationIdToContinueFrom, CancellationTokenSource cancellationTokenSource, Guid? contextId = null) 
            : base(runId, correlationIdToContinueFrom, cancellationTokenSource, contextId)
        {
            CausativeInput = causativeInput;
            Exception = exception;
        }
    }

    public sealed class ContinueCancelledFlowMessage : ContinueFlowMessage
    {
        public ContinueCancelledFlowMessage(RunId runId, Guid correlationIdToContinueFrom, CancellationTokenSource cancellationTokenSource, Guid? contextId = null) 
            : base(runId, correlationIdToContinueFrom, cancellationTokenSource, contextId)
        {
            
        }
    }

    public sealed class ContinueCompletedFlowMessage : ContinueFlowMessage
    {
        public readonly Result Parameter;

        public ContinueCompletedFlowMessage(RunId runId, Result parameter, Guid correlationIdToContinueFrom, CancellationTokenSource cancellationTokenSource, Guid? contextId = null)
            : base(runId, correlationIdToContinueFrom, cancellationTokenSource, contextId)
        {
            Parameter = parameter;
        }
    }
}