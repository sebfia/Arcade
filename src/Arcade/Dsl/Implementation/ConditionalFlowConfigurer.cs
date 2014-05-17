using System;

namespace Arcade.Dsl.Implementation
{
    public sealed class ConditionalFlowConfigurer<TIn> : BaseFlowConfigurer, IConditionalFlowConfigurer, IFlowConfigurer<TIn>
    {
        private readonly Delegate _condition;
        private readonly Func<IFlowConfigurer<TIn>, BranchEnd> _branch;

        public ConditionalFlowConfigurer(string flowName, IFlowConfigurer previous, Delegate condition, Func<IFlowConfigurer<TIn>, BranchEnd> branch)
            : base(flowName, previous)
        {
            _condition = condition;
            _branch = branch;
        }
        
        public Delegate Condition
        {
            get
            {
                return _condition;
            }
        }

        public IFlowConfigurer JoinFlowConfigurer
        {
            get
            {
                return _branch(this).Last;
            }
        }

        public string JoinpointName
        {
            get
            {
                return _branch(this).JoinpointName;
            }
        }
    }
}