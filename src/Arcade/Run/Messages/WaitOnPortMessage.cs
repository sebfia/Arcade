using System;
using System.Threading;
using Arcade.Run.Execution;

namespace Arcade.Run.Messages
{
    public sealed class WaitOnPortMessage : IRuntimeMessage
    {
        public readonly Guid CorrelationIdToContinueWith;
        public readonly string PortName;
        public readonly Result Input;
        public readonly CancellationTokenSource CancellationTokenSource;

        public WaitOnPortMessage(RunId runId, Guid correlationIdToContinueWith, string portName, Result input, CancellationTokenSource cancellationTokenSource, Guid? contextId = null)
        {
            CorrelationIdToContinueWith = correlationIdToContinueWith;
            PortName = portName;
            Input = input;
            CancellationTokenSource = cancellationTokenSource;
            RunId = runId;
            ContextId = contextId;
        }

        public RunId RunId { get; private set; }
        public Guid? ContextId { get; private set; }
    }
}