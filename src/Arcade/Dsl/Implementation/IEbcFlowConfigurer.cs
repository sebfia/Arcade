using System;
using Arcade.Run.RunVectors;

namespace Arcade.Dsl.Implementation
{
    public interface IEbcFlowConfigurer : IFlowConfigurer
    {
        Type FlowType { get; }
        BoxingContinuationBase CreateBoxingContinuation();
    }
}