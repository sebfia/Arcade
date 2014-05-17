using System;

namespace Arcade.Dsl.Implementation
{
    public interface IGatherFlowConfigurer : IFlowConfigurer
    {
        TreatExceptionsWhenGathering TreatExceptions { get; }
        Type GatheredResultType { get; }
    }
}