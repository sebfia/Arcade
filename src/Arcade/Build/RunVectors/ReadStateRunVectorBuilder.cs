using System;
using Arcade.Build.FlowStack;
using Arcade.Dsl;
using Arcade.Dsl.Implementation;
using Arcade.Run.RunVectors;

namespace Arcade.Build.RunVectors
{
    public sealed class ReadStateRunVectorBuilder : IRunVectorBuilder
    {
        public bool CanBuildRunVectorFromFlowConfigurer(IFlowConfigurer flowConfigurer)
        {
            return flowConfigurer is IReadStateFlowConfigurer;
        }

        public IRunVector BuildRunVector(IFlowConfigurer flowConfigurer, Guid nextCorrelationId, IStackBuilder stackBuilder)
        {
            var configurer = flowConfigurer as IReadStateFlowConfigurer;

            if (configurer == null)
                throw new InvalidOperationException("Can not build run vector from this flow configurer type!");

            var correlationId = configurer.CorrelationId;
            var stateType = configurer.StateType;
            var resultCombinator = configurer.Combine;

            return new ReadStateRunVector(correlationId, nextCorrelationId, stateType, resultCombinator);
        }
    }
}