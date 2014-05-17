using System;

namespace Arcade.Run.Execution.Events
{
    public sealed class WaitOnPortEvent : IRunFlowStackEvent
    {
        public readonly Guid CorrelationIdToContinueWith;
        public readonly string PortName;
        public readonly Result Input;

        public WaitOnPortEvent(RunId runId, Guid correlationIdToContinueWith, string portName, Result input)
        {
            RunId = runId;
            CorrelationIdToContinueWith = correlationIdToContinueWith;
            PortName = portName;
            Input = input;
        }

        public RunId RunId { get; private set; }
    }
}