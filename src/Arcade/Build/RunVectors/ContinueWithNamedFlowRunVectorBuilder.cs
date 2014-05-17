using System;
using Arcade.Build.FlowStack;
using Arcade.Dsl;
using Arcade.Dsl.Implementation;
using Arcade.Run.RunVectors;

namespace Arcade.Build.RunVectors
{
    public sealed class ContinueWithNamedFlowRunVectorBuilder : IRunVectorBuilder
    {
        public bool CanBuildRunVectorFromFlowConfigurer(IFlowConfigurer flowConfigurer)
        {
            return flowConfigurer is IContinueWithNamedFlowConfigurer;
        }

        public IRunVector BuildRunVector(IFlowConfigurer flowConfigurer, Guid nextCorrelationId, IStackBuilder stackBuilder)
        {
            var configurer = flowConfigurer as IContinueWithNamedFlowConfigurer;
            var correlationId = configurer.CorrelationId;
            var flowName = configurer.FlowNameToContinueWith;
            var timeout = 10.Seconds();

            return new ContinueWithNamedFlowRunVector(correlationId, nextCorrelationId, flowName, timeout);
        }
    }
    
}