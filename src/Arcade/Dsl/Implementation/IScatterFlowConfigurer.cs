namespace Arcade.Dsl.Implementation
{
    public interface IScatterFlowConfigurer : IFlowConfigurer
    {
        IGatherFlowConfigurer End { get; }
        IFlowConfigurer Start { get; }
    }
}