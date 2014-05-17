using System;
using Arcade.Build.FlowStack;
using Arcade.Dsl;
using Arcade.Dsl.Implementation;
using Arcade.Run.RunVectors;

namespace Arcade.Build.RunVectors
{
    public sealed class FunctionRunVectorBuilder : IRunVectorBuilder
    {
        private readonly TimeSpan _standardTimeout;

        public FunctionRunVectorBuilder(TimeSpan standardTimeout)
        {
            _standardTimeout = standardTimeout;
        }

        public bool CanBuildRunVectorFromFlowConfigurer(IFlowConfigurer flowConfigurer)
        {
            return flowConfigurer is IFunctionFlowConfigurer;
        }

        public IRunVector BuildRunVector(IFlowConfigurer flowConfigurer, Guid nextCorrelationId, IStackBuilder stackBuilder)
        {
            var configurer = flowConfigurer as IFunctionFlowConfigurer;
            var correlationId = configurer.CorrelationId;
            var function = configurer.Function;

            return new FunctionRunVector(correlationId, nextCorrelationId, function, _standardTimeout);
        }
    }
    
}