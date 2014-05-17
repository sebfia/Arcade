namespace Arcade.Dsl.Implementation
{
    public sealed class ScatterOperationFlowConfigurer<TIn> :  BaseFlowConfigurer, IFlowConfigurer<TIn>
    {
        public ScatterOperationFlowConfigurer(string flowName, IFlowConfigurer previous)
            : base(flowName, previous)
        {

        }
    }
}