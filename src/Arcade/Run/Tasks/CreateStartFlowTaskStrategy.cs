using System;
using System.Collections.Generic;
using System.Threading;
using Arcade.Build.FlowStack;
using Arcade.Run.Aspects;
using Arcade.Run.Execution;
using Arcade.Run.Messages;

namespace Arcade.Run.Tasks
{

	public sealed class CreateStartFlowTaskStrategy : ICreateTaskFromMessageStrategy
	{
		public bool CanCreateTaskFromMessage(IRuntimeMessage runtimeMessage)
		{
			return runtimeMessage is StartFlowMessage;
		}

		public ITask CreateTaskFromMessage(IRuntimeMessage runtimeMessage, IFlowStackBuilderFactory flowStackBuilderFactory, IStateStore stateStore, IEnumerable<IAspect> aspects, CancellationToken cancellationToken)
		{
			var startFlowMessage = runtimeMessage as StartFlowMessage;

			if(startFlowMessage == null) throw new InvalidOperationException("Can not run task with invalid runtime message!");

			var runId = startFlowMessage.RunId;
			var parameter = startFlowMessage.Parameter;
			var contextId = runtimeMessage.ContextId;

			var cts = CancellationTokenSource.CreateLinkedTokenSource(startFlowMessage.CancellationToken, cancellationToken);

			return new RunFlowTask(runId, parameter, flowStackBuilderFactory, stateStore, aspects, cts, contextId);
		}
	}
}