using System;

namespace Arcade.Dsl.Implementation
{
    public interface ITriggerFlowConfigurer : IFlowConfigurer
    {
        string TriggerName { get; }
        Delegate Selector { get; }
    }
}