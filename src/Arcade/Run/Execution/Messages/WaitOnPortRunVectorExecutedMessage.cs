using System;

namespace Arcade.Run.Execution.Messages
{
    public sealed class WaitOnPortRunVectorExecutedMessage : RunVectorExecutedMessageBase
    {
        private readonly Guid _nextCorrelationId;
        private readonly Result _result;
        private readonly TimeSpan _timeout;
        private readonly string _portName;

        public WaitOnPortRunVectorExecutedMessage(RunId runId, Guid correlationId, Guid nextCorrelationId,
                                                 string portName, Result result, TimeSpan timeout)
            : base(runId, correlationId)
        {
            _nextCorrelationId = nextCorrelationId;
            _portName = portName;
            _result = result;
            _timeout = timeout;
        }

        public Guid NextCorrelationId
        {
            get { return _nextCorrelationId; }
        }

        public Result Result
        {
            get { return _result; }
        }

        public TimeSpan Timeout
        {
            get { return _timeout; }
        }

        public string PortName
        {
            get { return _portName; }
        }
    }
}