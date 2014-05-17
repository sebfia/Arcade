using System;
using System.Text;

namespace Arcade.Run.RunVectors
{
    public sealed class ContinueWithNamedFlowRunVector : IRunVector
    {
        private readonly Guid _correlationId;
        private readonly Guid _nextCorrelationId;
        private readonly TimeSpan _timeout;
        private readonly string _flowName;

        public ContinueWithNamedFlowRunVector(Guid correlationId, Guid nextCorrelationId, string flowName, TimeSpan timeout)
        {
            _correlationId = correlationId;
            _nextCorrelationId = nextCorrelationId;
            _flowName = flowName;
            _timeout = timeout;
        }

        public Guid CorrelationId
        {
            get { return _correlationId; }
        }

        public string FlowName
        {
            get { return _flowName; }
        }

        public TimeSpan Timeout
        {
            get { return _timeout; }
        }

        public Guid NextCorrelationId
        {
            get { return _nextCorrelationId; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("Continuing with named subflow: {0}. Next CorrelationId thereafter: {1}", _flowName,
                            _nextCorrelationId.ToString());
            
            var result = sb.ToString();

            sb.Clear();

            return result;
        }
    }
}