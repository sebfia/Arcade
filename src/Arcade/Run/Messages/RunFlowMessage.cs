using System;
using System.Threading;
using Arcade.Run.Execution;

namespace Arcade.Run.Messages
{

	public sealed class RunFlowMessage : IRuntimeMessage
	{
		public readonly Result Parameter;
		public readonly CancellationTokenSource CancellationTokenSource;

		public RunFlowMessage(RunId runId, Result parameter, CancellationTokenSource cancellationTokenSource, Guid? contextId = null)
		{
			RunId = runId;
			Parameter = parameter;
			CancellationTokenSource = cancellationTokenSource;
			ContextId = contextId;
		}

		public override string ToString()
		{
			return RunId;
		}

		public RunId RunId { get; private set; }
		public Guid? ContextId { get; private set; }
	}
}