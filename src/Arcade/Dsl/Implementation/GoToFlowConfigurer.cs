using System;

namespace Arcade.Dsl.Implementation
{
    public sealed class GoToFlowConfigurer<TIn> : BaseFlowConfigurer, IGoToFlowConfigurer, IFlowConfigurer<TIn>
    {
        private readonly Delegate _condition;
        private readonly string _joinpointName;

        public GoToFlowConfigurer(string flowName, IFlowConfigurer previous, Delegate condition, string joinpointName)
            : base(flowName, previous)
        {
            _condition = condition;
            _joinpointName = joinpointName;
        }

        public Delegate Condition
        {
            get { return _condition; }
        }

        public string JoinpointName
        {
            get { return _joinpointName; }
        }
    }
}