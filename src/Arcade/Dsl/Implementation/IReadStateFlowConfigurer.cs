using System;

namespace Arcade.Dsl.Implementation
{
    public interface IReadStateFlowConfigurer : IFlowConfigurer
    {
        Type StateType { get; }
        Delegate Combine { get; }
    }
}