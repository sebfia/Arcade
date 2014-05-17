using System;
using System.Text;
using Arcade.Dsl;

namespace Arcade.Run.RunVectors
{
    public sealed class ScatterRunVector : IRunVector
    {
        private readonly Guid _correlationId;
        private readonly Guid _correlationIdToStartWith;
        private readonly Guid _nextCorrelationId;
        private readonly TreatExceptionsWhenGathering _treatExceptions;
        private readonly Type _gatheredResultType;

        public ScatterRunVector(Guid correlationId, Guid correlationIdToStartWith, Guid nextCorrelationId, Type gatheredResultType, TreatExceptionsWhenGathering treatExceptions)
        {
            _correlationId = correlationId;
            _correlationIdToStartWith = correlationIdToStartWith;
            _nextCorrelationId = nextCorrelationId;
            _gatheredResultType = gatheredResultType;
            _treatExceptions = treatExceptions;
        }

        public Guid CorrelationId
        {
            get { return _correlationId; }
        }

        public TimeSpan Timeout
        {
            get { return 3.Seconds(); }
        }

        public Guid NextCorrelationId
        {
            get { return _nextCorrelationId; }
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

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat(
                "Scattering, will be gathering results of type: {0}. Treating exceptions in gathered flows: {1}",
                _gatheredResultType.Name, _treatExceptions.ToString());

            var result = sb.ToString();

            sb.Clear();

            return result;
        }
    }
}