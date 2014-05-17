using System;
using System.Threading;
using Arcade.Run.Execution;

namespace Arcade.Run.Messages
{
    public sealed class InitializeScatterMessage : IRuntimeMessage
    {
        public readonly Guid OriginatingCorrelationId;
        public readonly Guid CorrelationIdToStartWith;
        public readonly Guid NextCorrelationId;
        public readonly Result Parameter;
        public readonly Type GatheredResultType;
        public readonly bool IgnoreExceptions;
        public readonly CancellationTokenSource CancellationTokenSource;

        public InitializeScatterMessage(RunId runId, Guid originatingCorrelationId, Guid correlationIdToStartWith, Guid nextCorrelationId, Result parameter, Type gatheredResultType, bool ignoreExceptions, CancellationTokenSource cancellationTokenSource, Guid? contextId)
        {
            OriginatingCorrelationId = originatingCorrelationId;
            CorrelationIdToStartWith = correlationIdToStartWith;
            NextCorrelationId = nextCorrelationId;
            Parameter = parameter;
            GatheredResultType = gatheredResultType;
            IgnoreExceptions = ignoreExceptions;
            CancellationTokenSource = cancellationTokenSource;
            RunId = runId;
            ContextId = contextId;
        }

        public RunId RunId { get; private set; }
        public Guid? ContextId { get; private set; }
    }
}