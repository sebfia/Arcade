using System;

namespace Arcade.Dsl.Implementation
{
    public sealed class JoinpointFlowConfigurer : IJoinpointFlowConfigurer
    {
        private readonly string _joinpointName;
        private readonly IFlowConfigurer _decorated;

        public JoinpointFlowConfigurer(IFlowConfigurer decorated, string joinpointName)
        {
            if(decorated == null)
                throw new ArgumentNullException("decorated", "Decorated flow must not be null!");

            if(String.IsNullOrEmpty(joinpointName))
                throw new ArgumentException("Joinpoint must have a name!", "joinpointName");

            _decorated = decorated;
            _joinpointName = joinpointName;
        }

        public string FlowName
        {
            get
            {
                return _decorated.FlowName;
            }
        }
        
        public Guid CorrelationId
        {
            get
            {
                return _decorated.CorrelationId;
            }
        }
        
        public IFlowConfigurer Previous
        {
            get
            {
                return _decorated.Previous;
            }
        }

        public string JoinpointName
        {
            get
            {
                return _joinpointName;
            }
        }

        public IFlowConfigurer DecoratedFlowConfigurer
        {
            get { return _decorated; }
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
                return CorrelationId.GetHashCode() ^ (FlowName != null ? FlowName.GetHashCode() : 0);
            }
        }
        
    }

    public sealed class JoinpointFlowConfigurer<TOut> : IFlowConfigurer<TOut>, IJoinpointFlowConfigurer
    {
        private readonly string _joinpointName;
        private readonly IFlowConfigurer _decorated;
        
        public JoinpointFlowConfigurer(IFlowConfigurer<TOut> decorated, string joinpointName)
        {
            if(decorated == null)
                throw new ArgumentNullException("decorated", "Decorated flow must not be null!");
            
            if(String.IsNullOrEmpty(joinpointName))
                throw new ArgumentException("Joinpoint must have a name!", "joinpointName");
            
            _decorated = decorated;
            _joinpointName = joinpointName;
        }
        
        public string FlowName
        {
            get
            {
                return _decorated.FlowName;
            }
        }
        
        public Guid CorrelationId
        {
            get
            {
                return _decorated.CorrelationId;
            }
        }
        
        public IFlowConfigurer Previous
        {
            get
            {
                return _decorated.Previous;
            }
        }
        
        public string JoinpointName
        {
            get
            {
                return _joinpointName;
            }
        }
        
        public IFlowConfigurer DecoratedFlowConfigurer
        {
            get { return _decorated; }
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
                return CorrelationId.GetHashCode() ^ (FlowName != null ? FlowName.GetHashCode() : 0);
            }
        }
    }
}