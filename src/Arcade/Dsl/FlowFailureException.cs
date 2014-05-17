using System;

namespace Arcade.Dsl
{
    public class FlowFailureException : Exception
    {
        private readonly string _flowName;
        private readonly Guid _correlationId;

        public FlowFailureException(string flowName, Guid correlationId, Exception inner)
            : base(String.Format("An unhandled exception occurred in flow with name: {0}", flowName), inner)
        {
            _flowName = flowName;
            _correlationId = correlationId;
        }

        public string FlowName
        {
            get { return _flowName; }
        }

        public Guid CorrelationId
        {
            get { return _correlationId; }
        }
    }
    
}