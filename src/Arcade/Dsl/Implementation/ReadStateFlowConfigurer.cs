using System;

namespace Arcade.Dsl.Implementation
{
    public sealed class ReadStateFlowConfigurer<T> : BaseFlowConfigurer, IReadStateFlowConfigurer, IFlowConfigurer<T>
    {
        private readonly Type _stateType;
        private readonly Delegate _combine;

        public ReadStateFlowConfigurer(string flowName, IFlowConfigurer previous, Type stateType, Delegate combine)
            : base(flowName, previous)
        {
            _stateType = stateType;
            _combine = combine;
        }

        public Type StateType
        {
            get { return _stateType; }
        }

        public Delegate Combine
        {
            get { return _combine; }
        }
    }
}