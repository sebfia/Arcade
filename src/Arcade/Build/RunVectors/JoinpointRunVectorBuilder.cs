using System;
using Arcade.Build.FlowStack;
using Arcade.Dsl;
using Arcade.Dsl.Implementation;
using Arcade.Run.RunVectors;

namespace Arcade.Build.RunVectors
{
    public sealed class JoinpointRunVectorBuilder : IRunVectorBuilder
    {
        public bool CanBuildRunVectorFromFlowConfigurer(IFlowConfigurer flowConfigurer)
        {
            return flowConfigurer is IJoinpointFlowConfigurer;
        }        

        public IRunVector BuildRunVector(IFlowConfigurer flowConfigurer, Guid nextCorrelationId, IStackBuilder stackBuilder)
        {
            var configurer = flowConfigurer as IJoinpointFlowConfigurer;
            var decorated = configurer.DecoratedFlowConfigurer;
            var joinpointName = configurer.JoinpointName;

            stackBuilder.SetCorrelationIdForJoinpointName(joinpointName, configurer.CorrelationId);

            var actualRunVector = stackBuilder.BuildRunVector(decorated, nextCorrelationId);

            return actualRunVector;
        }
    }
    
}