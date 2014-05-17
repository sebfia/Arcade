using System;
using Arcade.Dsl;
using Arcade.Run.Execution.Events;
using Arcade.Run.Execution.Messages;

namespace Arcade.Run.Execution.Post
{
    public sealed class ScatterRunVectorExecutedMessageStrategy : IRunVectorExecutedMessageStrategy
    {
        public bool CanTreatRunVectorExecutedMessage(IRunVectorExecutedMessage runVectorExecutedMessage)
        {
            return runVectorExecutedMessage is ScatterRunVectorExecutedMessage;
        }

        public void TreatRunVectorExecutedMessage(IRunVectorExecutedMessage runVectorExecutedMessage,
                                                  IFlowStackOrchestrator orchestrator)
        {
            var message = runVectorExecutedMessage as ScatterRunVectorExecutedMessage;

            if (message == null) throw new InvalidOperationException("Unable to treat this kind of IRunVectorExecutedMessage");

            var runId = message.RunId;
            var originatingCorrelationId = message.CorrelationId;
            var parameter = message.Parameter;
            var correlationIdToStartWith = message.CorrelationIdToStartWith;
            var correlationIdToContinueWithAfterGather = message.NextCorrelationId;
            var ignoreExceptions = (message.TreatExceptions == TreatExceptionsWhenGathering.ContinueFlow);
            var gatheredResultType = message.GatheredResultType;

            if(!parameter.IsEnumerable) throw new InvalidOperationException("Input to scatter from must be enumerable!");

            orchestrator.PropagateEvent(new InitializeScatterEvent(runId, originatingCorrelationId, correlationIdToStartWith, correlationIdToContinueWithAfterGather, parameter, gatheredResultType, ignoreExceptions));
        }
    }
}