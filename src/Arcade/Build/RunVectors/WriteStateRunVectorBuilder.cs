using System;
using Arcade.Build.FlowStack;
using Arcade.Dsl;
using Arcade.Dsl.Implementation;
using Arcade.Run.RunVectors;

namespace Arcade.Build.RunVectors
{
    public sealed class WriteStateRunVectorBuilder : IRunVectorBuilder
    {
        public bool CanBuildRunVectorFromFlowConfigurer(IFlowConfigurer flowConfigurer)
        {
            return flowConfigurer is IWriteStateFlowConfigurer;
        }

        public IRunVector BuildRunVector(IFlowConfigurer flowConfigurer, Guid nextCorrelationId, IStackBuilder stackBuilder)
        {
            var configurer = flowConfigurer as IWriteStateFlowConfigurer;

            if (configurer == null)
                throw new InvalidOperationException("Can not build run vector from this flow configurer type!");

            var correlationId = configurer.CorrelationId;
            var stateSelector = configurer.SelectState;
            var resultSelector = configurer.SelectOutput;

            return new WriteStateRunVector(correlationId, nextCorrelationId, stateSelector, resultSelector);
        }
    }
}