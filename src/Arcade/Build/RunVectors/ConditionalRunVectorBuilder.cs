using System;
using Arcade.Build.FlowStack;
using Arcade.Dsl;
using Arcade.Dsl.Implementation;
using Arcade.Run.RunVectors;

namespace Arcade.Build.RunVectors
{
    public sealed class ConditionalRunVectorBuilder : IRunVectorBuilder
    {
        public bool CanBuildRunVectorFromFlowConfigurer(IFlowConfigurer flowConfigurer)
        {
            return flowConfigurer is IConditionalFlowConfigurer;
        }

        public IRunVector BuildRunVector(IFlowConfigurer flowConfigurer, Guid nextCorrelationId, IStackBuilder stackBuilder)
        {
            var configurer = flowConfigurer as IConditionalFlowConfigurer;

            if (configurer == null)
                throw new InvalidOperationException("Can not build run vector from this flow configurer type!");

            var correlationId = configurer.CorrelationId;
            var condition = configurer.Condition;
            var last = configurer.JoinFlowConfigurer;

            if (configurer.CorrelationId == last.CorrelationId)
            {
                throw new InvalidOperationException("You are about to run into a stack overflow with this flow configuration. There is no flow following in the branch.");
            }

            var joinpointName = configurer.JoinpointName;
            var first = last.NextParentAfter(configurer);
            var correlationIdIfConditionIsTrue = first.CorrelationId;
            var correlationIdFollowingLast = stackBuilder.FindJoinCorrelationIdForJoinpointName(joinpointName);

            var conditional = new Conditional(condition, correlationIdIfConditionIsTrue, nextCorrelationId);
            var conditionalRunVector = new ConditionalRunVector(correlationId, conditional);
            stackBuilder.Traverse(first, last, correlationIdFollowingLast);

            return conditionalRunVector;
        }
    }
}