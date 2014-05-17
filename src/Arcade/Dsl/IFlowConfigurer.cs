using System;

namespace Arcade.Dsl
{
    public interface IFlowConfigurer : IEquatable<IFlowConfigurer>
    {
        Guid CorrelationId { get; }
        string FlowName { get; }
        IFlowConfigurer Previous { get; }
    }

    public interface IFlowConfigurer<T> : IFlowConfigurer
    {

    }
}