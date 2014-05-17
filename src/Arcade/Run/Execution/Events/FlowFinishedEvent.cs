namespace Arcade.Run.Execution.Events
{
    public sealed class FlowFinishedEvent : IRunFlowStackEvent
    {
        public readonly Result Result;

        public FlowFinishedEvent(RunId runId, Result result)
        {
            RunId = runId;
            Result = result;
        }

        public RunId RunId { get; private set; }
    }
}