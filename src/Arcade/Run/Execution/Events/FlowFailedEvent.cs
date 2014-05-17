using System;

namespace Arcade.Run.Execution.Events
{
    public sealed class FlowFailedEvent : IRunFlowStackEvent
    {
        public readonly Guid CorrelationId;
        public readonly object CausativeInput;
        public readonly Exception Reason;

        public FlowFailedEvent(RunId runId, Guid correlationId, object causativeInput, Exception reason)
        {
            RunId = runId;
            CorrelationId = correlationId;
            CausativeInput = causativeInput;
            Reason = reason;
        }

        public RunId RunId { get; private set; }
    }
}