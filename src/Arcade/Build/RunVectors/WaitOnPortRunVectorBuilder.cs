using System;
using Arcade.Build.FlowStack;
using Arcade.Dsl;
using Arcade.Dsl.Implementation;
using Arcade.Run.RunVectors;

namespace Arcade.Build.RunVectors
{
    public sealed class WaitOnPortRunVectorBuilder : IRunVectorBuilder
    {
        public bool CanBuildRunVectorFromFlowConfigurer(IFlowConfigurer flowConfigurer)
        {
            return flowConfigurer is IWaitOnPortFlowConfigurer;
        }

        public IRunVector BuildRunVector(IFlowConfigurer flowConfigurer, Guid nextCorrelationId, IStackBuilder stackBuilder)
        {
            var configurer = flowConfigurer as IWaitOnPortFlowConfigurer;

            if(configurer == null)
                throw new InvalidOperationException("Can not build run vector from this flow configurer type!");

            var correlationId = configurer.CorrelationId;
            var portName = configurer.PortName;
            var timeout = 60.Seconds();//not yet implemented or used

            return new WaitOnPortRunVector(correlationId, nextCorrelationId, portName, timeout);
        }
    }
}