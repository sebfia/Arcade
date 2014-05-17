using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arcade.Build.FlowStack;
using Arcade.Build.RunVectors;
using Arcade.Dsl;
using Arcade.Run.Aspects;
using Arcade.Run.Continuations;
using Arcade.Run.Execution;
using Arcade.Run.Messages;
using Arcade.Run.Observers;
using Arcade.Run.Ports;
using Arcade.Run.Tasks;
using Arcade.Run.Triggers;

namespace Arcade.Engine
{
    public static class ProducerConsumerExtensions
    {
        public static void Clear<T>(this IProducerConsumerCollection<T> value)
        {
            T item;

            while (value.TryTake(out item))
            {
                
            }
        }
    }

    public class RuntimeEngine
    {
        #region Observer
        private sealed class Observer
        {
            private readonly Delegate _method;
            private readonly object _target;

            public Observer(object target, Delegate method)
            {
                _target = target;
                _method = method;
            }

            public void Invoke(IRuntimeMessage runtimeMessage)
            {
                _method.DynamicInvoke(new object[]{ runtimeMessage });
            }
        }
        #endregion

        #region RuntimeEngineThread

        private sealed class RuntimeEngineThread : IDisposable
        {
            private readonly Task _task;
            private readonly AutoResetEvent _checkValve;
            
            public RuntimeEngineThread(Task task, AutoResetEvent checkValve)
            {
                _task = task;
                _checkValve = checkValve;
            }

            public Task Task
            {
                get { return _task; }
            }

            public AutoResetEvent CheckValve
            {
                get { return _checkValve; }
            }

            public void Dispose()
            {
                _checkValve.Dispose();
                _task.Dispose();
            }

            private bool Equals(RuntimeEngineThread other)
            {
                return _task.Equals(other._task);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is RuntimeEngineThread && Equals((RuntimeEngineThread) obj);
            }

            public override int GetHashCode()
            {
                return _task.GetHashCode();
            }

            public static bool operator ==(RuntimeEngineThread left, RuntimeEngineThread right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(RuntimeEngineThread left, RuntimeEngineThread right)
            {
                return !Equals(left, right);
            }
        }

        #endregion

        private readonly int _numberOfThreads;
        private readonly List<RuntimeEngineThread> _threads;
        private readonly ConcurrentQueue<IRuntimeMessage> _commandQueue;
        private readonly IFlowStackBuilderFactory _flowStackBuilderFactory;
        private readonly ConcurrentDictionary<Type, List<Observer>> _observers;
        private readonly LinkedList<ICreateTaskFromMessageStrategy> _taskCreators;

        private bool _started;
        private CancellationTokenSource _cts;

        private readonly FlowFinishedObserver _flowFinishedObserver;
        private readonly ScatterGatherObserver _scatterGatherObserver;
        private readonly PortContinuationObserver _portContinuationObserver;
        private readonly TriggerContinuationObserver _triggerContinuationObserver;
        private readonly ChildFlowObserver _childFlowObserver;
        private readonly ObservingStateStore _stateStore;
        private readonly IEnumerable<IAspect> _aspects;
        private event Action<RuntimeEngineExecutionException> _unhandledFailedFlow;

        public RuntimeEngine(int numberOfThreads, RuntimeConfiguration runtimeConfiguration, params FlowConfiguration[] configurations)
        {
            _numberOfThreads = numberOfThreads;
            _threads = new List<RuntimeEngineThread>(_numberOfThreads);
            _commandQueue = new ConcurrentQueue<IRuntimeMessage>();
            _flowStackBuilderFactory = new FlowStackBuilderFactory(new InstanceFactory(runtimeConfiguration), configurations, 5.Seconds());

            _aspects = runtimeConfiguration.GetAspects();

            _observers = new ConcurrentDictionary<Type, List<Observer>>();

            _taskCreators = new LinkedList<ICreateTaskFromMessageStrategy>();
            _taskCreators.AddLast(new CreateRunFlowTaskStrategy());
            _taskCreators.AddLast(new CreateContinueFlowTaskStrategy());
            _taskCreators.AddLast (new CreateStartFlowTaskStrategy ());

            _flowFinishedObserver = new FlowFinishedObserver();
            RegisterObserver<FlowCompleteMessage>(_flowFinishedObserver.Observe);
            RegisterObserver<FlowFailedMessage>(_flowFinishedObserver.Observe);
            RegisterObserver<FlowCancelledMessage>(_flowFinishedObserver.Observe);

            _portContinuationObserver = new PortContinuationObserver(Enqueue);
            RegisterObserver<WaitOnPortMessage>(_portContinuationObserver.Observe);
            RegisterObserver<PortResultMessage> (_portContinuationObserver.Observe);

            _triggerContinuationObserver = new TriggerContinuationObserver();
            RegisterObserver<TriggerMessage>(_triggerContinuationObserver.Observe);

            _childFlowObserver = new ChildFlowObserver(Enqueue);
            RegisterObserver<InitializeChildFlowMessage>(_childFlowObserver.Observe);
            RegisterObserver<FlowCompleteMessage>(_childFlowObserver.Observe);
            RegisterObserver<FlowFailedMessage>(_childFlowObserver.Observe);
            RegisterObserver<FlowCancelledMessage>(_childFlowObserver.Observe);

            _scatterGatherObserver = new ScatterGatherObserver(Enqueue);
            RegisterObserver<InitializeScatterMessage>(_scatterGatherObserver.Observe);
            RegisterObserver<FlowCompleteMessage>(_scatterGatherObserver.Observe);
            RegisterObserver<FlowFailedMessage>(_scatterGatherObserver.Observe);
            RegisterObserver<FlowCancelledMessage>(_scatterGatherObserver.Observe);

            _stateStore = new ObservingStateStore();
            RegisterObserver<FlowCompleteMessage>(_stateStore.Observe);
            RegisterObserver<FlowFailedMessage>(_stateStore.Observe);
            RegisterObserver<FlowCancelledMessage>(_stateStore.Observe);
        }
        
        public RuntimeEngine(RuntimeConfiguration runtimeConfiguration, params FlowConfiguration[] configurations) : 
            this(4, runtimeConfiguration, configurations)
        { }

        private ICreateTaskFromMessageStrategy GetCreateTaskStrategyForMessage(IRuntimeMessage runtimeMessage)
        {
            LinkedListNode<ICreateTaskFromMessageStrategy> current = _taskCreators.First;

            while (current != null && !current.Value.CanCreateTaskFromMessage(runtimeMessage))
            {
                current = current.Next;
            }

            return current == null ? null : current.Value;
        }

        private void ThrowIfRuntimeEngineNotRunning()
        {
            if(!_started)
                throw new InvalidOperationException("This operation is not allowed! The runtime engine is not started.");
        }

        private void ThrowIfNoFlowExistsWithName(string flowName)
        {
            if (flowName != "*")
            if (!_flowStackBuilderFactory.CanCreateFlowStackBuilderForFlowName(flowName))
                throw new FlowNotFoundException(flowName);
        }

        private void ThrowIfNoPortWithNameExistsOnFlowWithName(string portName, string flowName)
        {
            if(flowName == "*")
                throw new InvalidOperationException("Star as a placeholder for all flows is only allowed on triggers not ports!");
        }

        /// <summary>
        /// Creates an invoker for the flow to start when being invoked.
        /// </summary>
        /// <returns>
        /// The invoker that starts the respective flow.
        /// </returns>
        /// <param name='flowName'>
        /// The name of the flow to start when being invoked.
        /// </param>
        public Action CreateProcessor(string flowName)
        {
            ThrowIfNoFlowExistsWithName(flowName);

            return ()=>
            {
                ThrowIfRuntimeEngineNotRunning();
                var runId = RunId.New(flowName);
                var cmd = new StartFlowMessage(runId, Result.Empty, CancellationToken.None);
                Enqueue(cmd);
            };
        }

        /// <summary>
        /// Creates an invoker for the flow to start when being invoked.
        /// </summary>
        /// <returns>
        /// The invoker that starts the respective flow.
        /// </returns>
        /// <param name='flowName'>
        /// Flow name.
        /// </param>
        /// <typeparam name='T'>
        /// The name of the flow to start when being invoked.
        /// </typeparam>
        public Action<T> CreateProcessor<T>(string flowName)
        {
            ThrowIfNoFlowExistsWithName(flowName);

            return x => 
            {
                ThrowIfRuntimeEngineNotRunning();
                var runId = RunId.New(flowName);
                var cmd = new StartFlowMessage(runId, Result.FromValue(x), CancellationToken.None);
                Enqueue(cmd);
            };
        }

        /// <summary>
        /// Waits for the result of a completed flow and continues on the thread the flow was running on.
        /// </summary>
        /// <param name='flowName'>
        /// The name of the flow to wait for.
        /// </param>
        /// <param name='continueWithResult'>
        /// The method to continue with when the result is received.
        /// </param>
        /// <typeparam name='TResult'>
        /// The type of the result to wait for.
        /// </typeparam>
        public void WaitForResult<TResult>(string flowName, Action<TResult> continueWithResult, Action<Exception> onException = null)
        {
            ThrowIfNoFlowExistsWithName(flowName);

            var whenFailed = onException ?? OnUnhandledFailedFlow;

            var flowResultContinuation = new FlowResultContinuation<TResult>(flowName, continueWithResult, whenFailed);
            _flowFinishedObserver.AddFlowResultContinuation(flowResultContinuation);
        }

        private void OnUnhandledFailedFlow(Exception exception)
        {
            _unhandledFailedFlow(new RuntimeEngineExecutionException("Unhandled failed flow!", exception));
        }

        /// <summary>
        /// Waits for the result of a completed flow and continues on the thread the flow was running on.
        /// </summary>
        /// <param name='flowName'>
        ///     The name of the flow to wait for.
        /// </param>
        /// <param name='continueWithResult'>
        ///     The method to continue with when the result is received.
        /// </param>
        /// <typeparam name='TResult'>
        /// The type of the result to wait for.
        /// </typeparam>
        public void WaitForResultScheduled<TResult>(string flowName, Action<TResult> continueWithResult, Action<Exception> onException = null)
        {
            ThrowIfNoFlowExistsWithName(flowName);

            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            var whenFailed = onException ?? (ex => {});

            var flowResultContinuation = new ScheduledFlowResultContinuation<TResult>(flowName, continueWithResult, whenFailed, taskScheduler);
            _flowFinishedObserver.AddFlowResultContinuation(flowResultContinuation);
        }

        /// <summary>
        /// Waits for the completion of a flow and continues on the thread context the scheduler was created on.
        /// </summary>
        /// <param name='flowName'>
        /// The name of the flow to wait for.
        /// </param>
        /// <param name='continueWith'>
        /// The method to continue with when the result is received.
        /// </param>
        public void WaitForCompletion(string flowName, Action continueWith, Action<Exception> onException = null)
        {
            ThrowIfNoFlowExistsWithName(flowName);

            var whenFailed = onException ?? (ex => {});

            var flowResultContinuation = new FlowResultContinuation(flowName, continueWith, whenFailed);
            _flowFinishedObserver.AddFlowResultContinuation(flowResultContinuation);
        }

        /// <summary>
        /// Waits for the completion of a flow and continues on the thread context the scheduler was created on.
        /// </summary>
        /// <param name='flowName'>
        ///     The name of the flow to wait for.
        /// </param>
        /// <param name='continueWith'>
        ///     The method to continue with when the result is received.
        /// </param>
        public void WaitForCompletionScheduled(string flowName, Action continueWith, Action<Exception> onException = null)
        {
            ThrowIfNoFlowExistsWithName(flowName);

            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            var whenFailed = onException ?? (ex => {});

            var flowResultContinuation = new ScheduledFlowResultContinuation(flowName, continueWith, whenFailed, taskScheduler);
            _flowFinishedObserver.AddFlowResultContinuation(flowResultContinuation);
        }

        public Task<TResult> RunFlow<TIn, TResult>(TIn input, string flowName)
        {
            return RunFlow<TIn, TResult>(input, flowName, CancellationToken.None);
        }

        public Task<TResult> RunFlow<TIn, TResult>(TIn input, string flowName, CancellationToken cancellationToken)
        {
            ThrowIfNoFlowExistsWithName(flowName);

            var tcs = new TaskCompletionSource<TResult> ();

            var runId = RunId.New(flowName);
            var cmd = new StartFlowMessage(runId, Result.FromValue(input), cancellationToken);

            var flowContinuation = new OneTimeFlowResultContinuation<TResult>(flowName, runId, tcs, 
                (cancellationToken == CancellationToken.None));// dispose the cancellation token source if no user-supplied cancellation token given
            _flowFinishedObserver.AddFlowResultContinuation(flowContinuation);

            Enqueue(cmd);

            return tcs.Task;
        }

        public Task<TResult> RunFlow<TResult>(string flowName, CancellationToken cancellationToken)
        {
            ThrowIfNoFlowExistsWithName(flowName);

            var tcs = new TaskCompletionSource<TResult>();

            var runId = RunId.New(flowName);
            var cmd = new StartFlowMessage(runId, Result.Empty, cancellationToken);

            var flowContinuation = new OneTimeFlowResultContinuation<TResult>(flowName, runId, tcs,
                (cancellationToken == CancellationToken.None));// dispose the cancellation token source if no user-supplied cancellation token given
            _flowFinishedObserver.AddFlowResultContinuation(flowContinuation);

            Enqueue(cmd);

            return tcs.Task;
        }

        public Task<TResult> RunFlow<TResult>(string flowName)
        {
            return RunFlow<TResult>(flowName, CancellationToken.None);
        }

        /// <summary>
        /// Creates a port for a port-name that has been defined in a flow.
        /// </summary>
        /// <typeparam name="TIn">The input Input type to the port.</typeparam>
        /// <typeparam name="TOut">The continaution type Input for the port.</typeparam>
        /// <param name="flowName">The name of the flow this port is defined in.</param>
        /// <param name="portName">The name of the port in the flow.</param>
        /// <returns>The <see cref="IPort{TIn,TOut}"/> instance that can then be used to interact with the flow.</returns>
        public IPort<TIn, TOut> CreatePort<TIn, TOut>(string flowName, string portName)
        {
            ThrowIfNoFlowExistsWithName(flowName);
            ThrowIfNoPortWithNameExistsOnFlowWithName(portName, flowName);
            var portId = Guid.NewGuid();
            var continueWith = new Action<PortResultMessage>(_portContinuationObserver.Observe);
            var port = new Port<TIn, TOut>(portName, portId, continueWith, _portContinuationObserver.RemovePortContinuation);
            var portContinuation = new PortContinuation<TIn>(flowName, portName, portId, port.InvokePort);
            _portContinuationObserver.AddPortContinuation(portContinuation);

            return port;
        }

        /// <summary>
        /// Creates a port for a port-name that has been defined in a flow using the 
        /// <see cref="TaskScheduler"/> from the current synchronization context for interaction.
        /// </summary>
        /// <typeparam name="TIn">The input Input type to the port.</typeparam>
        /// <typeparam name="TOut">The continaution type Input for the port.</typeparam>
        /// <param name="flowName">The name of the flow this port is defined in.</param>
        /// <param name="portName">The name of the port in the flow.</param>
        /// <returns>The <see cref="IPort{TIn,TOut}"/> instance that can then be used to interact with the flow.</returns>
        public IPort<TIn, TOut> CreateScheduledPort<TIn, TOut>(string flowName, string portName)
        {
            ThrowIfNoFlowExistsWithName(flowName);
            ThrowIfNoPortWithNameExistsOnFlowWithName(portName, flowName);
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var portId = Guid.NewGuid();
            var continueWith = new Action<PortResultMessage>(Enqueue);
            var port = new Port<TIn, TOut>(portName, portId, continueWith, _portContinuationObserver.RemovePortContinuation);
            var portContinuation = new ScheduledPortContinuation<TIn>(flowName, portName, portId, port.InvokePort, scheduler);
            _portContinuationObserver.AddPortContinuation(portContinuation);

            return port;
        }

        /// <summary>
        /// Creates a <see cref="ITrigger{T}"/> for a trigger-name that has been defined within a flow.
        /// </summary>
        /// <typeparam name="T">The type of the input Input to the trigger.</typeparam>
        /// <param name="flowName">The name of the flow, this trigger is defined in.</param>
        /// <param name="triggerName">The name of the trigger in the flow.</param>
        /// <returns>The <see cref="ITrigger{T}"/> instance that can then be used to receive information from the flow.</returns>
        public ITrigger<T> CreateTrigger<T>(string flowName, string triggerName)
        {
            ThrowIfNoFlowExistsWithName(flowName);

            var triggerId = Guid.NewGuid();
            var trigger = new Trigger<T>(triggerName, triggerId, _portContinuationObserver.RemovePortContinuation);
            var triggerContinuation = new TriggerContinuation<T>(flowName, triggerName, triggerId, trigger.Invoke);
            _triggerContinuationObserver.AddTriggerContinuation(triggerContinuation);

            return trigger;
        }

        /// <summary>
        /// Creates a <see cref="ITrigger{T}"/> for a trigger-name that has been defined within a flow
        /// using the <see cref="TaskScheduler"/> of the current synchronization context to send information 
        /// to the trigger.
        /// </summary>
        /// <typeparam name="T">The type of the input Input to the trigger.</typeparam>
        /// <param name="flowName">The name of the flow, this trigger is defined in.</param>
        /// <param name="triggerName">The name of the trigger in the flow.</param>
        /// <returns>The <see cref="ITrigger{T}"/> instance that can then be used to receive information from the flow.</returns>
        public ITrigger<T> CreateScheduledTrigger<T>(string flowName, string triggerName)
        {
            ThrowIfNoFlowExistsWithName(flowName);

            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var triggerId = Guid.NewGuid();
            var trigger = new Trigger<T>(triggerName, triggerId, _portContinuationObserver.RemovePortContinuation);
            var portContinuation = new ScheduledTriggerContinuation<T>(flowName, triggerName, triggerId, trigger.Invoke, scheduler);
            _triggerContinuationObserver.AddTriggerContinuation(portContinuation);

            return trigger;
        }

        public void RegisterObserver<TRuntimeMessage>(Action<TRuntimeMessage> observe) where TRuntimeMessage : IRuntimeMessage
        {
            var invoker = (Delegate) observe;
            var target = invoker.Target;
            var messageType = typeof(TRuntimeMessage);
            var newObserver = new Observer(target, invoker);

            RegisterObserver(messageType, newObserver);
        }

        public void RegisterObserver<TRuntimeMessage>(IRuntimeMessageObserver<TRuntimeMessage> observer) where TRuntimeMessage : IRuntimeMessage
        {
            var messageType = typeof(TRuntimeMessage);
            var invoker = (Delegate) new Action<TRuntimeMessage>(observer.Observe);
            var target = invoker.Target;
            var newObserver = new Observer(target, invoker);

            RegisterObserver(messageType, newObserver);
        }

        public event Action<RuntimeEngineExecutionException> ExecutionException
        {
            add
            {
                _unhandledFailedFlow += value;
                RegisterObserver<TaskExecutionExceptionMessage>(msg => value(new RuntimeEngineExecutionException(msg.ToString(), msg.TriggeringException)));
                RegisterObserver<BuildingFlowFailedMessage>(msg => value(new RuntimeEngineExecutionException(msg.RunId, "Building flow stack for flow failed!", new BuildFlowStackException("Error building flow stack for flow! See reason for why!", msg.Reason))));
            }

            remove
            {
                _unhandledFailedFlow -= value;
            }
        }

        private void RegisterObserver(Type runtimeMessageType, Observer observer)
        {
            _observers.AddOrUpdate(runtimeMessageType,
                                   type => new List<Observer>(1) { observer },
            (type, oldList) => {
                var result = new List<Observer>((oldList.Count + 1));
                oldList.ForEach(result.Add);
                result.Add(observer);
                return result;
            });
        }

        /// <summary>
        /// Starts up the runtime engine.
        /// </summary>
        public void Start ()
        {
            if(_started)
                return;

            if (_cts != null)
                _cts.Dispose();

            _cts = new CancellationTokenSource();
            
            for (int i=0; i < _numberOfThreads; i++) 
            {
                AddNewRuntimeEngineThread(_cts.Token);
            }
            
            _started = true;
        }

        private void AddNewRuntimeEngineThread(CancellationToken cancellationToken)
        {
            var waitHandle = new AutoResetEvent(true);
            var task = Task.Factory.StartNew(() => Dequeue(cancellationToken, waitHandle), cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            task.ContinueWith(OnTaskFailure, TaskContinuationOptions.OnlyOnFaulted);

            _threads.Add(new RuntimeEngineThread(task, waitHandle));
        }

        private void OnTaskFailure(Task failedTask)
        {
            var triggeringException = (RuntimeEngineThreadFailedException)failedTask.Exception.InnerExceptions[0];
            var thread = _threads.First(y => y.Task == failedTask);
            _threads.Remove(thread);
            thread.Dispose();
            var token = _cts.Token;
            AddNewRuntimeEngineThread(token);
            Enqueue(new TaskExecutionExceptionMessage(triggeringException.RunId, "An exception occurred while running task in runtime engine. See TriggeringException for further details!", triggeringException.InnerException));
        }

        /// <summary>
        /// Stops the runtime engine.
        /// </summary>
        public void Stop ()
        {
            if(!_started)
                return;

            _cts.Cancel();
            _threads.ForEach(thread => thread.CheckValve.Set());

            try
            {
                Task.WaitAll(_threads.Select(thread => thread.Task).ToArray());
            }
            catch(AggregateException){}

            Debug.WriteLine(_threads.Select(x=>x.Task).Any(t=>!t.IsCanceled));

            _threads.ForEach (thread => thread.Dispose ());

            _commandQueue.Clear();
            _stateStore.Clear();
            _flowFinishedObserver.Clear ();
            _scatterGatherObserver.Clear ();
            _childFlowObserver.Clear ();
            _started = false;
        }

        public void Reset()
        {
            Stop();
            Start();
        }

        /// <summary>
        /// Enqueues a new runtime message into the commandQueue.
        /// </summary>
        /// <param name="runtimeMessage">The new runtime message.</param>
        private void Enqueue(IRuntimeMessage runtimeMessage)
        {
            _commandQueue.Enqueue(runtimeMessage);

            foreach (var thread in _threads)
            {
                thread.CheckValve.Set();
            }
        }
        
        /// <summary>
        /// Dequeues messaged from the command queue.
        /// </summary>
        /// <param name="token">This cancellation token is being used to cancel 
        /// all running flows within the runtime engin, when it is stopped</param>
        /// <param name="checkValve">The check valve avoids running the while-
        /// loop continuously without any message in the command queue.</param>
        private void Dequeue(CancellationToken token, WaitHandle checkValve)
        {
            while (!token.IsCancellationRequested)
            {
                IRuntimeMessage message;
                _commandQueue.TryDequeue(out message);

                if (message != null)
                {
                    try
                    {
                        var strategy = GetCreateTaskStrategyForMessage(message);

                        if (strategy != null)
                        {
                            var task = strategy.CreateTaskFromMessage(message, _flowStackBuilderFactory, _stateStore, _aspects, token);
                            task.Run(Enqueue);
                        }
                        else
                        {
                            HandleObserversForRuntimeMessage(message);
                        }
                    }
                    catch (Exception exception)
                    {
                        throw new RuntimeEngineThreadFailedException(message.RunId, exception);
                    }
                }
                else
                {
					Thread.Sleep (15);
                }
            }

            throw new OperationCanceledException(token);
        }

        /// <summary>
        /// Handles observers for a runtime message that has no <see cref="ICreateTaskFromMessageStrategy"/> defined.
        /// </summary>
        /// <param name="runtimeMessage"></param>
        private void HandleObserversForRuntimeMessage(IRuntimeMessage runtimeMessage)
        {
            var observers = GetObserversForRuntimeMessageType(runtimeMessage.GetType());
            observers.ForEach(observer => observer.Invoke(runtimeMessage));
        }

        /// <summary>
        /// Gets all observers for a certain runtime message.
        /// </summary>
        /// <param name="runtimeMessageType"></param>
        /// <returns></returns>
        private List<Observer> GetObserversForRuntimeMessageType(Type runtimeMessageType)
        {
            List<Observer> list;
            
            if (_observers.TryGetValue(runtimeMessageType, out list))
            {
                return list;
            }
            
            return Enumerable.Empty<Observer>().ToList();
        }
    }
}