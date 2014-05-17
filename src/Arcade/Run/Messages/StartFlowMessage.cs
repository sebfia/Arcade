using System;
using System.Threading;
using Arcade.Run.Execution;

namespace Arcade.Run.Messages
{
    public sealed class StartFlowMessage : IRuntimeMessage
    {
        public readonly Result Parameter;
        public readonly CancellationToken CancellationToken;

        public StartFlowMessage(RunId runId, Result parameter, CancellationToken cancellationToken, Guid? contextId = null)
        {
            RunId = runId;
            Parameter = parameter;
            CancellationToken = cancellationToken;
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