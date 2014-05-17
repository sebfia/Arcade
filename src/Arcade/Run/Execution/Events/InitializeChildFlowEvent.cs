using System;

namespace Arcade.Run.Execution.Events
{
    public sealed class InitializeChildFlowEvent : IRunFlowStackEvent
    {
        public readonly string ChildFlowName;
        public readonly Guid CorrelationIdToContinueWithOnParent;
        public readonly Result Parameter;

        public InitializeChildFlowEvent(RunId runId, string childFlowName, Guid correlationIdToContinueWithOnParent, Result parameter)
        {
            RunId = runId;
            ChildFlowName = childFlowName;
            CorrelationIdToContinueWithOnParent = correlationIdToContinueWithOnParent;
            Parameter = parameter;
        }

        public RunId RunId { get; private set; }
    }
}