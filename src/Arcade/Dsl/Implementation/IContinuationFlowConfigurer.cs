using System;
using Arcade.Run.RunVectors;

namespace Arcade.Dsl.Implementation
{
    public interface IContinuationFlowConfigurer : IFlowConfigurer
    {
        Type ContinuationType { get; }
        Type ImplementedInterfaceType { get; }
        BoxingContinuationBase CreateBoxingContinuation();
    }
    
}