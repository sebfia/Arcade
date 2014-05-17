namespace Arcade.Dsl.Implementation
{
    public interface IJoinpointFlowConfigurer : IFlowConfigurer
    {
        string JoinpointName { get; }
        IFlowConfigurer DecoratedFlowConfigurer { get; }
    }
}