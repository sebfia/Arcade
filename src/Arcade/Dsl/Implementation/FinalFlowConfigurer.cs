namespace Arcade.Dsl.Implementation
{
    public sealed class FinalFlowConfigurer : BaseFlowConfigurer
    {
        public FinalFlowConfigurer(string flowName, IFlowConfigurer previous)
            : base(flowName, previous)
        {
        }
    }
    
}