using System;
using System.Threading;
using Arcade.Run.Execution;
using Arcade.Run.Messages;
using Arcade.Run.Tasks.Projections;

namespace Arcade.Run.Tasks
{
    public sealed class ContinueFromCancelledFlowTask : ITask
    {
        private readonly Guid _correlationIdToContinueFrom;
        private readonly CancellationTokenSource _cts;
        private readonly Guid? _contextId;
        private readonly RunId _runId;

        public ContinueFromCancelledFlowTask(RunId runId, Guid correlationIdToContinueFrom, CancellationTokenSource cts, Guid? contextId)
        {
            _runId = runId;
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
            observe(new FlowCancelledMessage(_runId, _correlationIdToContinueFrom, _cts, _contextId));
        }

        public void AcceptVisitor(ITaskVisitor visitor)
        {
            
        }
    }
}