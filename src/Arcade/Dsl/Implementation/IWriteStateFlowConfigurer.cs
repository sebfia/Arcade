using System;

namespace Arcade.Dsl.Implementation
{
    public interface IWriteStateFlowConfigurer : IFlowConfigurer
    {
        Delegate SelectState { get; }
        Delegate SelectOutput { get; }
    }
}