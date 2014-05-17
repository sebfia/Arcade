using System;
using Arcade.Run.RunVectors;

namespace Arcade.Dsl.Implementation
{
    public sealed class EbcFlowConfigurer : BaseFlowConfigurer, IEbcFlowConfigurer
    {
        private readonly Type _flowType;

        public EbcFlowConfigurer(string flowName, Type flowType)
            : this(flowName, null, flowType)
        {

        }

        public EbcFlowConfigurer(string flowName, IFlowConfigurer previous, Type flowType)
            : base(flowName, previous)
        {
            _flowType = flowType;
        }

        public Type FlowType
        {
            get
            {
                return _flowType;
            }
        }

        public BoxingContinuationBase CreateBoxingContinuation()
        {
            return new BoxingContinuation();
        }
    }

    public sealed class EbcFlowConfigurer<T> : BaseFlowConfigurer, IFlowConfigurer<T>, IEbcFlowConfigurer
    {
        private readonly Type _flowType;
        
        public EbcFlowConfigurer(string flowName, Type flowType)
            : this(flowName, null, flowType)
        {
            
        }
        
        public EbcFlowConfigurer(string flowName, IFlowConfigurer previous, Type flowType)
            : base(flowName, previous)
        {
            _flowType = flowType;
        }
        
        public Type FlowType
        {
            get
            {
                return _flowType;
            }
        }
        
        public BoxingContinuationBase CreateBoxingContinuation()
        {
            return new BoxingContinuation<T>();
        }
    }

    public static class When
    {
        public static bool True(bool value)
        {
            return value;
        }

        public static bool False(bool value)
        {
            return (value == false);
        }
    }
}