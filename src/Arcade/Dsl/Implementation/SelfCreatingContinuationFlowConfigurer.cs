using System;
using Arcade.Run.RunVectors;

namespace Arcade.Dsl.Implementation
{
    public sealed class SelfCreatingContinuationFlowConfigurer : BaseFlowConfigurer, IContinuationFlowConfigurer, ISelfCreatingFlowConfigurer
    {
        private readonly Type _continuationType;
        private readonly Func<object> _createInstance;
        private readonly Type _implementedInterfaceType;

        public SelfCreatingContinuationFlowConfigurer(string flowName, IFlowConfigurer previous, Type continuationType, Type implementedInterfaceType, Func<object> createInstance)
            : base(flowName, previous)
        {
            _continuationType = continuationType;
            _createInstance = createInstance;
            _implementedInterfaceType = implementedInterfaceType;
        }

        public Type ContinuationType
        {
            get { return _continuationType; }
        }

        public Type ImplementedInterfaceType
        {
            get { return _implementedInterfaceType; }
        }

        public BoxingContinuationBase CreateBoxingContinuation()
        {
            return new BoxingContinuation();
        }

        public Func<object> CreateInstance
        {
            get { return _createInstance; }
        }
    }

    public sealed class SelfCreatingContinuationFlowConfigurer<T> : BaseFlowConfigurer, IFlowConfigurer<T>, IContinuationFlowConfigurer, ISelfCreatingFlowConfigurer
    {
        private readonly Type _continuationType;
        private readonly Type _implementedInterfaceType;
        private readonly Func<object> _createInstance;
        
        public SelfCreatingContinuationFlowConfigurer(string flowName, IFlowConfigurer previous, Type continuationType, Type implementedInterfaceType, Func<object> createInstance)
            : base(flowName, previous)
        {
            _continuationType = continuationType;
            _implementedInterfaceType = implementedInterfaceType;
            _createInstance = createInstance;
        }
        
        public Type ContinuationType
        {
            get { return _continuationType; }
        }
        
        public Type ImplementedInterfaceType
        {
            get { return _implementedInterfaceType; }
        }
        
        public BoxingContinuationBase CreateBoxingContinuation()
        {
            return new BoxingContinuation<T>();
        }
        
        public Func<object> CreateInstance
        {
            get { return _createInstance; }
        }
    }
}