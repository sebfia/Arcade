namespace Arcade.Dsl.Implementation
{
    public interface IWaitOnPortFlowConfigurer : IFlowConfigurer
    {
        string PortName { get; }
    }
}