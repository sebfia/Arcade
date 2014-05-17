using System;

namespace Arcade.Dsl.Implementation
{
    public interface IFunctionFlowConfigurer : IFlowConfigurer
    {
        Delegate Function { get; }
    }
}