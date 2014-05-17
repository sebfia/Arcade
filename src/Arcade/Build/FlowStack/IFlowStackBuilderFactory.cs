namespace Arcade.Build.FlowStack
{
    public interface IFlowStackBuilderFactory
    {
        IFlowStackBuilder CreateFlowStackBuilder(string flowName);
        bool CanCreateFlowStackBuilderForFlowName(string flowName);
    }
}