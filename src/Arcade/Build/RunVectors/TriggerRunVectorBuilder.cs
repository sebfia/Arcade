using System;
using Arcade.Build.FlowStack;
using Arcade.Dsl;
using Arcade.Dsl.Implementation;
using Arcade.Run.RunVectors;

namespace Arcade.Build.RunVectors
{
    public sealed class TriggerRunVectorBuilder : IRunVectorBuilder
    {
        public bool CanBuildRunVectorFromFlowConfigurer(IFlowConfigurer flowConfigurer)
        {
            return flowConfigurer is ITriggerFlowConfigurer;
        }

        public IRunVector BuildRunVector(IFlowConfigurer flowConfigurer, Guid nextCorrelationId, IStackBuilder stackBuilder)
        {
            var configurer = flowConfigurer as ITriggerFlowConfigurer;

            if (configurer == null)
                throw new InvalidOperationException("Can not build run vector from this flow configurer type!");

            var correlationId = configurer.CorrelationId;
            var triggerName = configurer.TriggerName;
            var selector = configurer.Selector;

            return new TriggerRunVector(correlationId, nextCorrelationId, triggerName, selector);
        }
    }
}