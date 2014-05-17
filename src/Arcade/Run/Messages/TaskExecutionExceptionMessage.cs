using System;
using Arcade.Run.Execution;

namespace Arcade.Run.Messages
{
    public sealed class TaskExecutionExceptionMessage : IRuntimeMessage
    {
        public readonly Exception TriggeringException;
        private readonly string _message;

        public TaskExecutionExceptionMessage(RunId runId, string message, Exception triggeringException)
        {
            RunId = runId;
            _message = message;
            TriggeringException = triggeringException;
            ContextId = null;
        }

        public override string ToString()
        {
            return _message;
        }

        public RunId RunId { get; private set; }
        public Guid? ContextId { get; private set; }
    }
}