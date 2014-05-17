using System;

namespace Arcade.Run.Execution.Messages
{
    public class GatherRunVectorExecutedMessage : RunVectorExecutedMessageBase
    {
        private readonly Result _result;

        public GatherRunVectorExecutedMessage(RunId runId, Guid correlationId, Result result)
            : base(runId, correlationId)
        {
            _result = result;
        }

        public Result Result
        {
            get { return _result; }
        }
    }
}