namespace Arcade.Run.Execution.Events
{
    public sealed class GatherFlowResultEvent : IRunFlowStackEvent
    {
        public readonly Result Result;

        public GatherFlowResultEvent(RunId runId, Result result)
        {
            RunId = runId;
            Result = result;
        }

        public RunId RunId { get; private set; }
    }
}