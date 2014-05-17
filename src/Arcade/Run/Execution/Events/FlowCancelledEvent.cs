using System;

namespace Arcade.Run.Execution.Events
{
    public sealed class FlowCancelledEvent : IRunFlowStackEvent
    {
        public readonly Guid CorrelationId;

        public FlowCancelledEvent(RunId runId, Guid correlationId)
        {
            RunId = runId;
            CorrelationId = correlationId;
        }

        public RunId RunId { get; private set; }
    }
}