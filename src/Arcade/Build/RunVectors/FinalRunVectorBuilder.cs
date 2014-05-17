using System;
using Arcade.Build.FlowStack;
using Arcade.Dsl;
using Arcade.Dsl.Implementation;
using Arcade.Run.RunVectors;

namespace Arcade.Build.RunVectors
{
    public sealed class FinalRunVectorBuilder : IRunVectorBuilder
    {
        public bool CanBuildRunVectorFromFlowConfigurer(IFlowConfigurer flowConfigurer)
        {
            return flowConfigurer is FinalFlowConfigurer;
        }        

        public IRunVector BuildRunVector(IFlowConfigurer flowConfigurer, Guid nextCorrelationId, IStackBuilder stackBuilder)
        {
            stackBuilder.SetCorrelationIdForJoinpointName(String.Empty, flowConfigurer.CorrelationId);
            return new FinalRunVector(flowConfigurer.CorrelationId);
        }
    }
    
}