using System;
using Arcade.Build.FlowStack;
using Arcade.Dsl;
using Arcade.Run.RunVectors;

namespace Arcade.Build.RunVectors
{
    public interface IRunVectorBuilder
    {
        bool CanBuildRunVectorFromFlowConfigurer(IFlowConfigurer flowConfigurer);
        IRunVector BuildRunVector(IFlowConfigurer flowConfigurer, Guid nextCorrelationId, IStackBuilder stackBuilder);
    }
}