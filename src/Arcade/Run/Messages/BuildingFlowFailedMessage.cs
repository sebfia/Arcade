using System;
using Arcade.Run.Execution;

namespace Arcade.Run.Messages
{
    public sealed class BuildingFlowFailedMessage : IRuntimeMessage
    {
        public readonly Exception Reason;

        public BuildingFlowFailedMessage(RunId runId, Exception reason)
        {
            RunId = runId;
            Reason = reason;
            ContextId = null;
        }

        public RunId RunId { get; private set; }
        public Guid? ContextId { get; private set; }
    }
    	
}