using System;

namespace Arcade.Engine
{
    public class ScatterFlowFailedException : Exception
    {
        private readonly Guid _originatingCorrelationId;
        private readonly Guid _failedCorrelationId;
        private readonly object _causativeInput;

        public ScatterFlowFailedException(Guid originatingCorrelationId, Guid failedCorrelationId, Exception inner, object causativeInput) : base("An exception was thrown while a scatter flow was being executed.", inner)
        {
            _originatingCorrelationId = originatingCorrelationId;
            _failedCorrelationId = failedCorrelationId;
            _causativeInput = causativeInput;
        }

        public Guid OriginatingCorrelationId
        {
            get { return _originatingCorrelationId; }
        }

        public Guid FailedCorrelationId
        {
            get { return _failedCorrelationId; }
        }

        public object CausativeInput
        {
            get { return _causativeInput; }
        }
    }
}