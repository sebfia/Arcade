using System;
using Arcade.Run.RunVectors;

namespace Arcade.Dsl.Implementation
{
    public sealed class SelfCreatingEbcFlowConfigurer : BaseFlowConfigurer, IEbcFlowConfigurer, ISelfCreatingFlowConfigurer
    {
        private readonly Type _flowType;
        private readonly Func<object> _createInstance;

        public SelfCreatingEbcFlowConfigurer(string flowName, Type flowType, Func<object> createInstance)
            : this(flowName, null, flowType, createInstance)
        {

        }
        
        public SelfCreatingEbcFlowConfigurer(string flowName, IFlowConfigurer previous, Type flowType, Func<object> createInstance)
            : base(flowName, previous)
        {
            _flowType = flowType;
            _createInstance = createInstance;
        }

        public Type FlowType
        {
            get
            {
                return _flowType;
            }
        }
        
        public Func<object> CreateInstance
        {
            get
            {
                return _createInstance;
            }
        }
        
        public BoxingContinuationBase CreateBoxingContinuation()
        {
            return new BoxingContinuation();
        }
    }

    public sealed class SelfCreatingEbcFlowConfigurer<T> : BaseFlowConfigurer, IFlowConfigurer<T>, IEbcFlowConfigurer, ISelfCreatingFlowConfigurer
    {
        private readonly Type _flowType;
        private readonly Func<object> _createInstance;
        
        public SelfCreatingEbcFlowConfigurer(string name, Type flowType, Func<object> createInstance)
            : this(name, null, flowType, createInstance)
        {
            
        }
        
        public SelfCreatingEbcFlowConfigurer(string name, IFlowConfigurer previous, Type flowType, Func<object> createInstance)
            : base(name, previous)
        {
            _flowType = flowType;
            _createInstance = createInstance;
        }
        
        public Type FlowType
        {
            get
            {
                return _flowType;
            }
        }
        
        public Func<object> CreateInstance
        {
            get
            {
                return _createInstance;
            }
        }
        
        public BoxingContinuationBase CreateBoxingContinuation()
        {
            return new BoxingContinuation<T>();
        }
    }
}