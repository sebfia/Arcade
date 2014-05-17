using System;

namespace Arcade.Run.Execution.Messages
{
    public sealed class ContinueWithNamedFlowRunVectorExecutedMessage : RunVectorExecutedMessageBase
    {
        private readonly string _flowName;
        private readonly Guid _nextCorrelationId;
        private readonly Result _result;

        public ContinueWithNamedFlowRunVectorExecutedMessage(RunId runId, Guid correlationId, Result result, string flowName, Guid nextCorrelationId)
            : base(runId, correlationId)
        {
            _result = result;
            _flowName = flowName;
            _nextCorrelationId = nextCorrelationId;
        }

        public string FlowName
        {
            get { return _flowName; }
        }

        public Guid NextCorrelationId
        {
            get { return _nextCorrelationId; }
        }

        public Result Result
        {
            get { return _result; }
        }
    }
    
}