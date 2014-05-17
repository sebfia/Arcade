using System;

namespace Arcade.Run.Execution.Messages
{
    public sealed class ExecuteRunVectorMessage
    {
        public ExecuteRunVectorMessage(RunId runId, Guid correlationId, Result parameter)
        {
            RunId = runId;
            CorrelationId = correlationId;
            Parameter = parameter;
        }

        public readonly RunId RunId;
        public readonly Guid CorrelationId;
        public readonly Result Parameter;
    }
    
}