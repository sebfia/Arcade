using System;

namespace Arcade.Run.Execution.Events
{
    public sealed class InitializeScatterEvent : IRunFlowStackEvent
    {
        public readonly Guid OriginatingCorrelationId;
        public readonly Result Parameter;
        public readonly Guid CorrelationIdToStartWith;
        public readonly Guid NextCorrelationId;
        public readonly bool IgnoreExceptions;
        public readonly Type GatheredResultType;

        public InitializeScatterEvent(RunId runId, Guid originatingCorrelationId, Guid correlationIdToStartWith, Guid correlationIdToContinueWithAfterGather, Result parameter, Type gatheredResultType, bool ignoreExceptions)
        {
            RunId = runId;
            OriginatingCorrelationId = originatingCorrelationId;
            CorrelationIdToStartWith = correlationIdToStartWith;
            NextCorrelationId = correlationIdToContinueWithAfterGather;
            Parameter = parameter;
            GatheredResultType = gatheredResultType;
            IgnoreExceptions = ignoreExceptions;
        }

        public RunId RunId { get; private set; }
    }
}