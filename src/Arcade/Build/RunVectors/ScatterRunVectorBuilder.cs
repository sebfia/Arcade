using System;
using Arcade.Build.FlowStack;
using Arcade.Dsl;
using Arcade.Dsl.Implementation;
using Arcade.Run.RunVectors;

namespace Arcade.Build.RunVectors
{
    public sealed class ScatterRunVectorBuilder : IRunVectorBuilder
    {
        public bool CanBuildRunVectorFromFlowConfigurer(IFlowConfigurer flowConfigurer)
        {
            return flowConfigurer is IScatterFlowConfigurer;
        }

        public IRunVector BuildRunVector(IFlowConfigurer flowConfigurer, Guid nextCorrelationId, IStackBuilder stackBuilder)
        {
            var configurer = flowConfigurer as IScatterFlowConfigurer;

            if (configurer == null) throw new InvalidOperationException("Can not build run vector from this flow configurer type!");

            var correlationId = configurer.CorrelationId;
            var last = configurer.End;
            var gatheredResultType = last.GatheredResultType;
            var first = last.NextParentAfter(configurer.Start);

            var result = new ScatterRunVector(correlationId, first.CorrelationId, nextCorrelationId, gatheredResultType, last.TreatExceptions);

            stackBuilder.Traverse(first, last, nextCorrelationId);

            return result;
        }
    }
}