using System;

namespace Arcade.Dsl.Implementation
{
    public abstract class BaseFlowConfigurer : IFlowConfigurer
    {
        private readonly Guid _correlationId;
        private readonly string _flowName;
        private readonly IFlowConfigurer _previous;

        protected BaseFlowConfigurer(string flowName, IFlowConfigurer previous)
        {
            if (String.IsNullOrWhiteSpace(flowName)) throw new ArgumentException("Invalid name for flow!", "flowName");

            _flowName = flowName;
            _previous = previous;
            _correlationId = CreateNewCorrelationId();
        }

        public string FlowName
        {
            get
            {
                return _flowName;
            }
        }

        public Guid CorrelationId
        {
            get
            {
                return _correlationId;
            }
        }

        public IFlowConfigurer Previous
        {
            get
            {
                return _previous;
            }
        }

        private static Guid CreateNewCorrelationId()
        {
            return Guid.NewGuid();
        }

        public bool Equals(IFlowConfigurer other)
        {
            if(other == null)
                return false;

            return CorrelationId.Equals(other.CorrelationId) && FlowName.Equals(other.FlowName);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as IFlowConfigurer);
        }
        

        public override int GetHashCode()
        {
            unchecked
            {
                return _correlationId.GetHashCode() ^ (_flowName != null ? _flowName.GetHashCode() : 0);
            }
        }
        
    }
}