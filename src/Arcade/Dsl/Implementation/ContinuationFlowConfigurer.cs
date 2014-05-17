using System;
using Arcade.Run.RunVectors;

namespace Arcade.Dsl.Implementation
{
    public sealed class ContinuationFlowConfigurer<T> : BaseFlowConfigurer, IFlowConfigurer<T>, IContinuationFlowConfigurer
    {
        private readonly Type _continuationType;
        private readonly Type _implementedInterfaceType;

        public ContinuationFlowConfigurer(string flowName, IFlowConfigurer previous, Type type, Type implementedInterfaceType)
            : base(flowName, previous)
        {
            _continuationType = type;
            _implementedInterfaceType = implementedInterfaceType;
        }

        public Type ContinuationType
        {
            get
            {
                return _continuationType;
            }
        }

        public Type ImplementedInterfaceType
        {
            get { return _implementedInterfaceType; }
        }

        public BoxingContinuationBase CreateBoxingContinuation()
        {
            return new BoxingContinuation<T>();
        }
    }

    public sealed class ContinuationFlowConfigurer : BaseFlowConfigurer, IContinuationFlowConfigurer
    {
        private readonly Type _continuationType;
        private readonly Type _implementedInterfaceType;

        public ContinuationFlowConfigurer(string flowName, IFlowConfigurer previous, Type type, Type implementedInterfaceType)
            : base(flowName, previous)
        {
            _continuationType = type;
            _implementedInterfaceType = implementedInterfaceType;
        }

        public Type ContinuationType
        {
            get
            {
                return _continuationType;
            }
        }

        public Type ImplementedInterfaceType
        {
            get { return _implementedInterfaceType; }
        }

        public BoxingContinuationBase CreateBoxingContinuation()
        {
            return new BoxingContinuation();
        }
    }
}