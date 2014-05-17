using System;
using System.Collections.Generic;
using System.Threading;
using Arcade.Build.FlowStack;
using Arcade.Run.Aspects;
using Arcade.Run.Execution;
using Arcade.Run.Messages;
using Arcade.Run.Tasks.Projections;

namespace Arcade.Run.Tasks
{
    public sealed class ContinueFromSuccessfulFlowTask : ITask
    {
        private readonly Guid _correlationIdToContinueFrom;
        private readonly Result _input;
        private readonly IFlowStackBuilderFactory _flowStackBuilderFactory;
        private readonly IStateStore _stateStore;
        private readonly IEnumerable<IAspect> _aspects;
        private readonly CancellationTokenSource _cts;
        private readonly Guid? _contextId;
        private readonly RunId _runId;

        public ContinueFromSuccessfulFlowTask(RunId runId, Result input, Guid correlationIdToContinueFrom, IFlowStackBuilderFactory flowStackBuilderFactory, IStateStore stateStore, IEnumerable<IAspect> aspects, CancellationTokenSource cts, Guid? contextId)
        {
            _runId = runId;
            _input = input;
            _correlationIdToContinueFrom = correlationIdToContinueFrom;
            _flowStackBuilderFactory = flowStackBuilderFactory;
            _stateStore = stateStore;
            _aspects = aspects;
            _cts = cts;
            _contextId = contextId;
        }

        public RunId RunId
        {
            get { return _runId; }
        }

        public void Run(Action<IRuntimeMessage> observe)
        {
            IFlowStack flowStack;
            
            using (var builder = _flowStackBuilderFactory.CreateFlowStackBuilder(_runId.ToString()))
            {
                try
                {
                    flowStack = builder.BuildUpFlowStack();
                }
                catch (Exception ex)
                {
                    observe(new BuildingFlowFailedMessage(_runId, ex));
                    return;
                }
            }

            var projection = Projection.Standard.Build(this);
            
            using (var runFlowStack = CreateRunFlowStack(flowStack))
            {
                runFlowStack.ProgressPort += evt =>
                    {
                        var msg = projection(evt);
                        observe(msg);
                    };

                runFlowStack.RunFlowStackFromCorrelationId(_correlationIdToContinueFrom, _input);
            }
        }

        public void AcceptVisitor(ITaskVisitor visitor)
        {
            visitor.SetCancellationTokenSource(_cts);
            visitor.SetContextId(_contextId);
        }

        private IRunFlowStack CreateRunFlowStack(IFlowStack flowStack)
        {
            return new SimpleFlowStackOrchestrator(flowStack, _runId, _stateStore, _aspects, _cts.Token);
        }
    }
}