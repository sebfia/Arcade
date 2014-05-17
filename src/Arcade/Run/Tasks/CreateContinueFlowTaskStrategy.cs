using System;
using System.Collections.Generic;
using System.Threading;
using Arcade.Build.FlowStack;
using Arcade.Run.Aspects;
using Arcade.Run.Execution;
using Arcade.Run.Messages;

namespace Arcade.Run.Tasks
{
    public sealed class CreateContinueFlowTaskStrategy : ICreateTaskFromMessageStrategy
    {
        public bool CanCreateTaskFromMessage(IRuntimeMessage runtimeMessage)
        {
            return runtimeMessage is ContinueFlowMessage;
        }

        public ITask CreateTaskFromMessage(IRuntimeMessage runtimeMessage, IFlowStackBuilderFactory flowStackBuilderFactory,
                                           IStateStore stateStore, IEnumerable<IAspect> aspects, CancellationToken cancellationToken)
        {
            var continueMessage = runtimeMessage as ContinueFlowMessage;

            if (continueMessage == null) throw new InvalidOperationException("Can not run task with invalid runtime message!");

            var runId = continueMessage.RunId;
            var contextId = continueMessage.ContextId;
            var correlationIdToContinueFrom = continueMessage.CorrelationIdToContinueFrom;

            var cts = continueMessage.CancellationTokenSource;

            var completedMessage = continueMessage as ContinueCompletedFlowMessage;
            var failedMessage = continueMessage as ContinueFailedFlowMessage;

            if (completedMessage != null)
            {
                return new ContinueFromSuccessfulFlowTask(
                    runId, 
                    completedMessage.Parameter, 
                    correlationIdToContinueFrom, 
                    flowStackBuilderFactory, 
                    stateStore, 
                    aspects, 
                    cts, 
                    contextId);
            }

            if (failedMessage != null)
            {
                return new ContinueFromFailedFlowTask(
                    runId,
                    failedMessage.Exception,
                    failedMessage.CausativeInput,
                    correlationIdToContinueFrom,
                    cts,
                    contextId);
            }
            
            return new ContinueFromCancelledFlowTask(
                runId,
                correlationIdToContinueFrom,
                cts,
                contextId);
        }
    }
}