using System;
using Arcade.Build.FlowStack;
using Arcade.Dsl;
using Arcade.Dsl.Implementation;
using Arcade.Run.RunVectors;

namespace Arcade.Build.RunVectors
{
    public sealed class GoToRunVectorBuilder : IRunVectorBuilder
    {
        public bool CanBuildRunVectorFromFlowConfigurer(IFlowConfigurer flowConfigurer)
        {
            return flowConfigurer is IGoToFlowConfigurer;
        }

        public IRunVector BuildRunVector(IFlowConfigurer flowConfigurer, Guid nextCorrelationId, IStackBuilder stackBuilder)
        {
            var configurer = flowConfigurer as IGoToFlowConfigurer;

            if (configurer == null)
                throw new InvalidOperationException("Can not build run vector from this flow configurer type!");

            var correlationId = configurer.CorrelationId;
            var evaluator = configurer.Condition;
            var joinpointName = configurer.JoinpointName;
            var correlationIdIfConditionTrue = stackBuilder.FindJoinCorrelationIdForJoinpointName(joinpointName);

            var conditional = new Conditional(evaluator, correlationIdIfConditionTrue, nextCorrelationId);

            return new ConditionalRunVector(correlationId, conditional);
        }
    }
}