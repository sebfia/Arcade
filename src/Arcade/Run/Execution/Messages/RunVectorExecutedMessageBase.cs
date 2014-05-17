using System;

namespace Arcade.Run.Execution.Messages
{
    public abstract class RunVectorExecutedMessageBase : IRunVectorExecutedMessage
    {
        private readonly RunId _runId;
        private readonly Guid _correlationId;

        protected RunVectorExecutedMessageBase(RunId runId, Guid correlationId)
        {
            _runId = runId;
            _correlationId = correlationId;
        }

        public Guid CorrelationId
        {
            get
            {
                return _correlationId;
            }
        }

        public RunId RunId
        {
            get { return _runId; }
        }
    }
    
}