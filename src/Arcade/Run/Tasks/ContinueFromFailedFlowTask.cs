using System;
using System.Threading;
using Arcade.Run.Execution;
using Arcade.Run.Messages;
using Arcade.Run.Tasks.Projections;

namespace Arcade.Run.Tasks
{
    public sealed class ContinueFromFailedFlowTask : ITask
    {
        private readonly Guid _correlationIdToContinueFrom;
        private readonly Exception _exception;
        private readonly object _causativeInput;
        private readonly CancellationTokenSource _cts;
        private readonly Guid? _contextId;
        private readonly RunId _runId;

        public ContinueFromFailedFlowTask(RunId runId, Exception exception, object causativeInput, Guid correlationIdToContinueFrom, CancellationTokenSource cts, Guid? contextId)
        {
            _runId = runId;
            _exception = exception;
            _causativeInput = causativeInput;
            _correlationIdToContinueFrom = correlationIdToContinueFrom;
            _cts = cts;
            _contextId = contextId;
        }

        public RunId RunId
        {
            get { return _runId; }
        }

        public void Run(Action<IRuntimeMessage> observe)
        {
            observe(new FlowFailedMessage(_runId, _exception, _causativeInput, _correlationIdToContinueFrom, _cts, _contextId));
        }

        public void AcceptVisitor(ITaskVisitor visitor)
        {
            
        }
    }
}