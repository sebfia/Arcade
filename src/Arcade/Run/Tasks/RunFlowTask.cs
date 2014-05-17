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
    public sealed class RunFlowTask : ITask
    {
        private readonly Result _input;
        private readonly IFlowStackBuilderFactory _flowStackBuilderFactory;
        private readonly RunId _runId;
        private readonly IStateStore _stateStore;
        private readonly IEnumerable<IAspect> _aspects;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Guid? _contextId;

        public RunFlowTask(RunId runId, Result input, IFlowStackBuilderFactory flowStackBuilderFactory, IStateStore stateStore, IEnumerable<IAspect> aspects, CancellationTokenSource cancellationTokenSource, Guid? contextId)
        {
            _runId = runId;
            _input = input;
            _flowStackBuilderFactory = flowStackBuilderFactory;
            _stateStore = stateStore;
            _aspects = aspects;
            _cancellationTokenSource = cancellationTokenSource;
            _contextId = contextId;
        }

        public RunId RunId
        {
            get { return _runId; }
        }

        public void Run(Action<IRuntimeMessage> observe)
        {
            IFlowStack flowStack;

            using (var builder = _flowStackBuilderFactory.CreateFlowStackBuilder(_runId))
            {
                try
                {
                    flowStack = builder.BuildUpFlowStack();
                } catch (Exception ex)
                {
                    observe(new BuildingFlowFailedMessage(_runId, ex));
                    return;
                }
            }

            var process = Projection.Standard.Build(this);

            using (var runFlowStack = CreateRunFlowStack(flowStack))
            {
                runFlowStack.ProgressPort += evt =>
                    {
                        var msg = process(evt);
                        observe(msg);
                    };

                runFlowStack.RunFlowStack(_input);
            }
        }

        public void AcceptVisitor(ITaskVisitor visitor)
        {
            visitor.SetCancellationTokenSource(_cancellationTokenSource);
            visitor.SetContextId(_contextId);
        }

        private IRunFlowStack CreateRunFlowStack(IFlowStack flowStack)
        {
            return new SimpleFlowStackOrchestrator(flowStack, _runId, _stateStore, _aspects, _cancellationTokenSource.Token);
        }
    }
}