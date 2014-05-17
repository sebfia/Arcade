using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Arcade.Run.Aspects;
using Arcade.Run.Execution.Events;
using Arcade.Run.Execution.Messages;
using Arcade.Run.Execution.Post;
using Arcade.Run.Execution.Pre;
using Arcade.Run.RunVectors;

namespace Arcade.Run.Execution
{
    public sealed class SimpleFlowStackOrchestrator : IFlowStackOrchestrator, IRunFlowStack
    {
        private readonly IFlowStack _flowStack;
        private readonly RunId _runId;
        private readonly IStateStore _stateStore;
        private readonly CancellationToken _cancellationToken; 
        private readonly LinkedList<IRunVectorExecutionStrategy> _runVectorExecutionStrategies;
        private readonly LinkedList<IRunVectorExecutedMessageStrategy> _executedMessageStrategies;
        private readonly List<IPreExecutionAdvice> _preExecutionAdvices;
        private readonly List<IPostExecutionAdvice> _postExecutionAdvices;

        public SimpleFlowStackOrchestrator(IFlowStack flowStack, RunId runId, IStateStore stateStore, IEnumerable<IAspect> aspects, CancellationToken cancellationToken)
        {
            _flowStack = flowStack;
            _runId = runId;
            _stateStore = stateStore;
            _cancellationToken = cancellationToken;

            _runVectorExecutionStrategies = new LinkedList<IRunVectorExecutionStrategy>();

            _runVectorExecutionStrategies.AddLast(new ExecutableRunVectorExecutionStrategy());
            _runVectorExecutionStrategies.AddLast(new TriggerRunVectorExecutionStrategy());
            _runVectorExecutionStrategies.AddLast(new ScatterRunVectorExecutionStrategy());
            _runVectorExecutionStrategies.AddLast(new GatherRunVectorExecutionStrategy());
            _runVectorExecutionStrategies.AddLast(new FinalRunVectorExecutionStrategy());
            _runVectorExecutionStrategies.AddLast(new ContinueWithNamedFlowRunVectorExecutionStrategy());
            _runVectorExecutionStrategies.AddLast(new WaitOnPortRunVectorExecutionStrategy());

            _executedMessageStrategies = new LinkedList<IRunVectorExecutedMessageStrategy>();

            _executedMessageStrategies.AddLast(new SuccessfulRunVectorExecutedMessageStrategy());
            _executedMessageStrategies.AddLast(new TriggerRunVectorExecutedMessageStrategy());
            _executedMessageStrategies.AddLast(new ScatterRunVectorExecutedMessageStrategy());
            _executedMessageStrategies.AddLast(new GatherRunVectorExecutedMessageStrategy());
            _executedMessageStrategies.AddLast(new FinalRunVectorExecutedMessageStrategy());
            _executedMessageStrategies.AddLast(new WaitOnPortRunVectorExecutedMessageStrategy());
            _executedMessageStrategies.AddLast(new ContinueWithNamedFlowExecutedMessageStrategy());
            _executedMessageStrategies.AddLast(new ExceptionRunVectorExecutedMessageStrategy());
            _executedMessageStrategies.AddLast(new TimeoutRunVectorExecutedMessageStrategy());
            _executedMessageStrategies.AddLast(new CancelledRunVectorExecutedMessageStrategy());

            var aspectArray = aspects != null ? aspects.ToArray() : new IAspect[0];

            _preExecutionAdvices = aspectArray.OfType<IPreExecutionAdvice>().ToList();

            _postExecutionAdvices = aspectArray.OfType<IPostExecutionAdvice>().ToList();

            ProgressPort += msg => { };
        }

        public void RunFlowStack(Result parameter)
        {
            var runVector = GetFirstRunVector();

            var package = CreateExecutePackage(runVector, parameter);

            RunExecutePackage(package);
        }

        public void RunFlowStackFromCorrelationId(Guid correlationId, Result parameter)
        {
            var runVector = GetRunVectorForCorrelationId(correlationId);

            var package = CreateExecutePackage(runVector, parameter);

            RunExecutePackage(package);
        }

        private void RunExecutePackage(ExecutePackage executePackage)
        {
            ProcessPreAdvices(executePackage);

            var executionStrategy = FindExecutionStrategyForPackage(executePackage);
            
            executionStrategy.ExecutePackage(executePackage, RunNextInStack);
        }

        private void RunNextInStack(IRunVectorExecutedMessage executedMessage)
        {
            ProcessPostAdvices(executedMessage);

            if (_cancellationToken.IsCancellationRequested) //the flow has been cancelled by the runtime engine.
            {
                PropagateEvent(new FlowCancelledEvent(_runId, executedMessage.CorrelationId));
                return;
            }

            var strategy = FindStrategyForRunVectorExecutedMessage(executedMessage);
            strategy.TreatRunVectorExecutedMessage(executedMessage, this);
        }

        public void RunAfterSuccessfulTerminationOfPrevious(Guid nextCorrelationId, Result previousResult)
        {
            var runVector = GetRunVectorForCorrelationId(nextCorrelationId);
            var executePackage = CreateExecutePackage(runVector, previousResult);

            RunExecutePackage(executePackage);
        }

        public void PropagateEvent(IRunFlowStackEvent runFlowStackEvent)
        {
            ProgressPort(runFlowStackEvent);
        }

        private void ProcessPreAdvices(ExecutePackage executePackage)
        {
            foreach (var preExecutionAdvice in _preExecutionAdvices.Where(preExecutionAdvice => preExecutionAdvice.Handles(executePackage)))
            {
                preExecutionAdvice.Handle(executePackage);
            }
        }

        private void ProcessPostAdvices(IRunVectorExecutedMessage executedMessage)
        {
            foreach (var postExecutionAdvice in _postExecutionAdvices.Where(postExecutionAdvice => postExecutionAdvice.Handles(executedMessage)))
            {
                postExecutionAdvice.Handle(executedMessage);
            }
        }

        private IRunVectorExecutedMessageStrategy FindStrategyForRunVectorExecutedMessage(IRunVectorExecutedMessage executedMessage)
        {
            var current = _executedMessageStrategies.First;

            while (current != null && !current.Value.CanTreatRunVectorExecutedMessage(executedMessage))
            {
                current = current.Next;
            }

            if(current == null)
                throw new InvalidOperationException("No strategy defined to treat RunVectorExecutedMessage!");

            return current.Value;
        }

        private IRunVectorExecutionStrategy FindExecutionStrategyForPackage(ExecutePackage executePackage)
        {
            var current = _runVectorExecutionStrategies.First;

            while (current != null && !current.Value.CanExecutePackage(executePackage))
            {
                current = current.Next;
            }

            if(current == null)
                throw new InvalidOperationException("No strategy defined to execute ExecutePackage!");

            return current.Value;
        }

        private IRunVector GetFirstRunVector()
        {
            return _flowStack.GetFirstRunVector();
        }

        private IRunVector GetRunVectorForCorrelationId(Guid correlationId)
        {
            return _flowStack.GetRunVectorForCorrelationId(correlationId);
        }

        private ExecutePackage CreateExecutePackage(IRunVector runVector, Result previousResult)
        {
            var executeMessage = new ExecuteRunVectorMessage(_runId, runVector.CorrelationId, previousResult);
            return new ExecutePackage(runVector, _stateStore, executeMessage, _cancellationToken);
        }

        public event Action<IRunFlowStackEvent> ProgressPort;

        public void Dispose()
        {
            var eventInfo = GetType().GetEvent("ProgressPort");
            var invokers = ProgressPort.GetInvocationList();

            foreach (var invoker in invokers)
            {
                eventInfo.RemoveEventHandler(this, invoker);
            }
        }
    }
    
}