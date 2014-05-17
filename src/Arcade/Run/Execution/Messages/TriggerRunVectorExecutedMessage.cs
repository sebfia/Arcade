using System;

namespace Arcade.Run.Execution.Messages
{
    public sealed class TriggerRunVectorExecutedMessage : RunVectorExecutedMessageBase
    {
        private readonly Guid _nextCorrelationId;
        private readonly string _triggerName;
        private readonly Result _result;
        private readonly Result _selectedResult;

        public TriggerRunVectorExecutedMessage(RunId runId, Guid correlationId, Guid nextCorrelationId, string triggerName, Result result, Result selectedResult)
            : base(runId, correlationId)
        {
            _nextCorrelationId = nextCorrelationId;
            _triggerName = triggerName;
            _result = result;
            _selectedResult = selectedResult;
        }

        public Guid NextCorrelationId
        {
            get { return _nextCorrelationId; }
        }

        public string TriggerName
        {
            get { return _triggerName; }
        }

        public Result Result
        {
            get { return _result; }
        }

        public Result SelectedResult
        {
            get { return _selectedResult; }
        }
    }
}