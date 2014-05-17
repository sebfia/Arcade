namespace Arcade.Run.Execution.Events
{
    public sealed class TriggerEvent : IRunFlowStackEvent
    {
        public readonly string TriggerName;
        public readonly Result Parameter;

        public TriggerEvent(RunId runId, string triggerName, Result parameter)
        {
            RunId = runId;
            TriggerName = triggerName;
            Parameter = parameter;
        }

        public RunId RunId { get; private set; }
    }
    
}