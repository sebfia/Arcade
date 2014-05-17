using System;
using Arcade.Dsl;

namespace Arcade.Run.Execution.Messages
{
    public class ScatterRunVectorExecutedMessage : RunVectorExecutedMessageBase
    {
        private readonly Result _parameter;
        private readonly Guid _correlationIdToStartWith;
        private readonly Guid _nextCorrelationId;
        private readonly TreatExceptionsWhenGathering _treatExceptions;

        private readonly TimeSpan _timeout;
        private readonly Type _gatheredResultType;

        public ScatterRunVectorExecutedMessage(RunId runId, Guid correlationId, Result parameter, Guid correlationIdToStartWith, Guid nextCorrelationId, Type gatheredResultType, TreatExceptionsWhenGathering treatExceptions, TimeSpan timeout)
            : base(runId, correlationId)
        {
            _parameter = parameter;
            _correlationIdToStartWith = correlationIdToStartWith;
            _nextCorrelationId = nextCorrelationId;
            _treatExceptions = treatExceptions;
            _timeout = timeout;
            _gatheredResultType = gatheredResultType;
        }

        public Result Parameter
        {
            get { return _parameter; }
        }

        public Guid NextCorrelationId
        {
            get { return _nextCorrelationId; }
        }

        public TimeSpan Timeout
        {
            get { return _timeout; }
        }

        public Guid CorrelationIdToStartWith
        {
            get { return _correlationIdToStartWith; }
        }

        public TreatExceptionsWhenGathering TreatExceptions
        {
            get { return _treatExceptions; }
        }

        public Type GatheredResultType
        {
            get { return _gatheredResultType; }
        }
    }
}