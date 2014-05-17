using System;

namespace Arcade.Dsl.Implementation
{
    public interface IGoToFlowConfigurer : IFlowConfigurer
    {
        Delegate Condition { get; }
        string JoinpointName { get; }
    }
}