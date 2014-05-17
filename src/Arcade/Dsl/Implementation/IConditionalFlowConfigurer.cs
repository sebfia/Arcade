using System;

namespace Arcade.Dsl.Implementation
{
    public interface IConditionalFlowConfigurer : IFlowConfigurer
    {
        Delegate Condition { get; }
        IFlowConfigurer JoinFlowConfigurer { get; }
        string JoinpointName { get; }
    }
    
}