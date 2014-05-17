using System;
using System.Threading;
using Arcade.Run.Execution;

namespace Arcade.Run.Messages
{
    public sealed class InitializeChildFlowMessage : IRuntimeMessage
    {
        public readonly Guid CorrelationIdToContinueWithOnParent;
        public readonly string ChildFlowName;
        public readonly Result Parameter;
        public readonly CancellationTokenSource CancellationTokenSource;

        public InitializeChildFlowMessage(RunId runId, Guid correlationIdToContinueWithOnParent, string childFlowName, Result parameter, CancellationTokenSource cancellationTokenSource, Guid? contextId = null)
        {
            CorrelationIdToContinueWithOnParent = correlationIdToContinueWithOnParent;
            ChildFlowName = childFlowName;
            Parameter = parameter;
            CancellationTokenSource = cancellationTokenSource;
            RunId = runId;
            ContextId = contextId;
        }

        public RunId RunId { get; private set; }
        public Guid? ContextId { get; private set; }
    }
}