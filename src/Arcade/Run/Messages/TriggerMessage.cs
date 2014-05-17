using System;
using Arcade.Run.Execution;

namespace Arcade.Run.Messages
{
    public sealed class TriggerMessage : IRuntimeMessage
    {
        public readonly string TriggerName;
        public readonly Result Result;

        public TriggerMessage(RunId runId, string triggerName, Result result)
        {
            TriggerName = triggerName;
            Result = result;
            RunId = runId;
            ContextId = null;
        }

        public RunId RunId { get; private set; }
        public Guid? ContextId { get; private set; }
    }
}