using System;
using System.Collections.Generic;
using System.Threading;
using Arcade.Build.FlowStack;
using Arcade.Run.Aspects;
using Arcade.Run.Execution;
using Arcade.Run.Messages;

namespace Arcade.Run.Tasks
{
    public sealed class CreateRunFlowTaskStrategy : ICreateTaskFromMessageStrategy
    {
        public bool CanCreateTaskFromMessage(IRuntimeMessage runtimeMessage)
        {
            return runtimeMessage is RunFlowMessage;
        }

        public ITask CreateTaskFromMessage(IRuntimeMessage runtimeMessage, IFlowStackBuilderFactory flowStackBuilderFactory, IStateStore stateStore, IEnumerable<IAspect> aspects, CancellationToken cancellationToken)
        {
            var runFlowMessage = runtimeMessage as RunFlowMessage;

            if(runFlowMessage == null) throw new InvalidOperationException("Can not run task with invalid runtime message!");

            var runId = runFlowMessage.RunId;
            var parameter = runFlowMessage.Parameter;
            var contextId = runtimeMessage.ContextId;
            var cts = runFlowMessage.CancellationTokenSource;

            return new RunFlowTask(runId, parameter, flowStackBuilderFactory, stateStore, aspects, cts, contextId);
        }
    }
}