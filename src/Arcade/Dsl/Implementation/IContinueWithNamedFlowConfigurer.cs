namespace Arcade.Dsl.Implementation
{
    public interface IContinueWithNamedFlowConfigurer : IFlowConfigurer
    {
        string FlowNameToContinueWith { get; }
    }
    
}