using System;
using Arcade.Build.FlowStack;
using Arcade.Dsl;
using Arcade.Dsl.Implementation;
using Arcade.Run.RunVectors;

namespace Arcade.Build.RunVectors
{
    public sealed class GatherRunVectorBuilder : IRunVectorBuilder
    {
        public bool CanBuildRunVectorFromFlowConfigurer(IFlowConfigurer flowConfigurer)
        {
            return flowConfigurer is IGatherFlowConfigurer;
        }

        public IRunVector BuildRunVector(IFlowConfigurer flowConfigurer, Guid nextCorrelationId, IStackBuilder stackBuilder)
        {
            var configurer = flowConfigurer as IGatherFlowConfigurer;

            if (configurer == null) throw new InvalidOperationException("Can not build run vector from this flow configurer type!");

            return new GatherRunVector(configurer.CorrelationId);
        }
    }
}