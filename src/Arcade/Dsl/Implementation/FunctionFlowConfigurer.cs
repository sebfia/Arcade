using System;

namespace Arcade.Dsl.Implementation
{
    public sealed class FunctionFlowConfigurer<T> : BaseFlowConfigurer, IFunctionFlowConfigurer, IFlowConfigurer<T>
    {
        private readonly Delegate _function;

        public FunctionFlowConfigurer(string flowName, IFlowConfigurer previous, Delegate function)
            : base(flowName, previous)
        {
            _function = function;
        }

        public Delegate Function
        {
            get { return _function; }
        }
    }
}