using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Arcade.Engine;
using Arcade.Run.Execution;
using Arcade.Run.Messages;

namespace Arcade.Run.Observers
{
    public sealed class ScatterGatherObserver :
        IRuntimeMessageObserver<InitializeScatterMessage>,
        IRuntimeMessageObserver<FlowCompleteMessage>,
        IRuntimeMessageObserver<FlowFailedMessage>,
        IRuntimeMessageObserver<FlowCancelledMessage>
    {
        #region GatheringSlot
        private sealed class GatheringSlot : IDisposable
        {
            private volatile int _numberOfItemsMissing;
            private readonly Guid? _previousContextId;
            private readonly RunId _runIdToContinueOnAfterGathering;
            private readonly Guid _originatingCorrelationId;
            private readonly Guid _correlationIdAfterGathering;
            private readonly bool _ignoreExceptions;
            private readonly Type _gatheredResultType;
            private volatile Result[] _gatheredItems;
            private volatile Exception[] _gatheredExceptions;
            private readonly HashSet<RunId> _associatedChildren;
            private readonly CancellationTokenSource _scatterCts;
            private readonly CancellationTokenSource _linkedCts;
            private readonly CancellationTokenSource _originalCts;
            private volatile bool _isDone;

            public GatheringSlot(int numberOfItemsToGather, Guid? previousContextId, RunId runIdToContinueOnAfterGathering, Guid originatingCorrelationId, Guid correlationIdAfterGathering, Type gatheredResultType, bool ignoreExceptions, CancellationTokenSource originalCts, CancellationTokenSource scatterCts, CancellationTokenSource linkedCts)
            {
                _numberOfItemsMissing = numberOfItemsToGather;
                _previousContextId = previousContextId;
                _runIdToContinueOnAfterGathering = runIdToContinueOnAfterGathering;
                _originatingCorrelationId = originatingCorrelationId;
                _correlationIdAfterGathering = correlationIdAfterGathering;
                _gatheredResultType = gatheredResultType;
                _ignoreExceptions = ignoreExceptions;
                _scatterCts = scatterCts;
                _linkedCts = linkedCts;
                _originalCts = originalCts;
                _isDone = false;
                _gatheredItems = new Result[0];
                _gatheredExceptions = new Exception[0];
                _associatedChildren = new HashSet<RunId>();
            }

            private static T[] ArrayCopyAdd<T>(T[] old, T item)
            {
                var length = old.Length;
                var result = new T[length + 1];
                Array.Copy(old, result, length);
                result[length] = item;
                return result;
            }

            /// <summary>
            /// The message is only touched once the gathering is finished.
            /// </summary>
            /// <param name="result"></param>
            /// <param name="message"></param>
            public bool PushGatheredResult(Result result, ref IRuntimeMessage message)
            {
                int currentValue, newValue;

                do
                {
                    currentValue = _numberOfItemsMissing;
                    newValue = currentValue - 1;
                } while (Interlocked.CompareExchange(ref _numberOfItemsMissing, newValue, currentValue) != currentValue);

                lock (_gatheredItems)
                {
                    _gatheredItems = ArrayCopyAdd(_gatheredItems, result);
                }

                var done = _isDone;

                if (newValue == 0 && done == false)
                {
                    _isDone = true;
                    message = PrepareGatherMessage();
                }

                return newValue == 0;
            }

            public bool PushGatheredException(Guid correlationId, Exception exception, object causativeInput, ref IRuntimeMessage message)
            {
                int currentValue, newValue;

                do
                {
                    currentValue = _numberOfItemsMissing;
                    newValue = currentValue - 1;
                } while (Interlocked.CompareExchange(ref _numberOfItemsMissing, newValue, currentValue) != currentValue);

                lock (_gatheredExceptions)
                {
                    _gatheredExceptions = ArrayCopyAdd(_gatheredExceptions, new ScatterFlowFailedException(_originatingCorrelationId, correlationId, exception, causativeInput));
                }

                var done = _isDone;

                if ((newValue == 0 || !_ignoreExceptions) && done == false)
                {
                    _isDone = true;

                    if(newValue > 0 && !_ignoreExceptions)
                        _scatterCts.Cancel();

                    message = PrepareGatherMessage();
                }

                return newValue == 0;
            }

            public bool PushGatheredCancellation(ref IRuntimeMessage message)
            {
                int currentValue, newValue;

                do
                {
                    currentValue = _numberOfItemsMissing;
                    newValue = currentValue - 1;
                } while (Interlocked.CompareExchange(ref _numberOfItemsMissing, newValue, currentValue) != currentValue);

                var done = _isDone;

                if (done == false)
                {
                    _isDone = true;

                    message = new FlowCancelledMessage(_runIdToContinueOnAfterGathering, _originatingCorrelationId, _originalCts);
                }

                return newValue == 0;
            }

            private IRuntimeMessage PrepareGatherMessage()
            {
                if(_gatheredExceptions.Length > 0)
                    if (!_ignoreExceptions)
                    {
                        var aggregateException = new AggregateException("One or more flows failed within a scatter operation!", _gatheredExceptions);

                        return new FlowFailedMessage(_runIdToContinueOnAfterGathering, aggregateException, null, _originatingCorrelationId, _originalCts, _previousContextId);
                    }

                var array = Array.CreateInstance(_gatheredResultType, _gatheredItems.Length);

                for (int i = 0; i < array.Length; i++)
                {
                    array.SetValue(_gatheredItems[i].Value, i);
                }

                var gatheredResult = new Result(array);

                return new ContinueCompletedFlowMessage(_runIdToContinueOnAfterGathering, gatheredResult, _correlationIdAfterGathering, _originalCts, _previousContextId);
            }

            public void AddAssociatedChildRunId(RunId childRunId)
            {
                _associatedChildren.Add (childRunId);
            }

            public bool HasAssociatedChildRunId(RunId runId)
            {
                return _associatedChildren.Contains (runId);
            }

            public void Dispose()
            {
                //_gatheredItems = null;
                //_gatheredExceptions = null;
                _linkedCts.Dispose ();
                _scatterCts.Dispose();
                _associatedChildren.Clear ();
            }
        }
        #endregion

        private readonly Action<IRuntimeMessage> _enqueueRuntimeMessage;
        private readonly ConcurrentDictionary<Guid, GatheringSlot> _gatheringSlots;

        public ScatterGatherObserver(Action<IRuntimeMessage> enqueueRuntimeMessage)
        {
            _enqueueRuntimeMessage = enqueueRuntimeMessage;
            _gatheringSlots = new ConcurrentDictionary<Guid, GatheringSlot>();
        }

        public void Observe(InitializeScatterMessage runtimeMessage)
        {
            var correlationIdToStartWith = runtimeMessage.CorrelationIdToStartWith;
            var nextCorrelationId = runtimeMessage.NextCorrelationId;
            var originatingCorrelationId = runtimeMessage.OriginatingCorrelationId;
            var enumerableParameter = runtimeMessage.Parameter;
            var originatingRunId = runtimeMessage.RunId;
            var previousContextId = runtimeMessage.ContextId;
            var originalCts = runtimeMessage.CancellationTokenSource;

            var contextId = Guid.NewGuid();
            
            var gatheredResultType = runtimeMessage.GatheredResultType;
            var ignoreExceptions = runtimeMessage.IgnoreExceptions;

            var items = enumerableParameter.EnumerateValue();

            var numberOfItemsToGather = items.Length;

            if (numberOfItemsToGather == 0)
            {
                var resultArray = Array.CreateInstance(gatheredResultType, 0);
                var result = new Result(resultArray);
                _enqueueRuntimeMessage(new ContinueCompletedFlowMessage(originatingRunId, result, nextCorrelationId, originalCts, previousContextId));
                return;
            }

            var scatterCts = new CancellationTokenSource ();
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource (originalCts.Token, scatterCts.Token);

            var slot = ReserveSlot(contextId, previousContextId, numberOfItemsToGather, originatingRunId, originatingCorrelationId, nextCorrelationId, gatheredResultType, ignoreExceptions, originalCts, scatterCts, linkedCts);

            foreach (var parameter in items)
            {
                var childRunId = originatingRunId.NewChild(originatingRunId);
                slot.AddAssociatedChildRunId (childRunId);
                _enqueueRuntimeMessage(new ContinueCompletedFlowMessage(childRunId, parameter, correlationIdToStartWith, linkedCts, contextId));
            }
        }

        public void Observe(FlowCompleteMessage runtimeMessage)
        {
//			System.Diagnostics.Debug.WriteLine ("SGObserver INBOUND: " + runtimeMessage.Result.Value.ToString ());

            if(!runtimeMessage.ContextId.HasValue)
                return;

//			System.Diagnostics.Debug.WriteLine ("SGObserver OBSERVING: " + runtimeMessage.Result.Value.ToString ());

            GatheringSlot slot;
            if (_gatheringSlots.TryGetValue(runtimeMessage.ContextId.Value, out slot))//if not this scatter has most probably been cancelled
            {
                if (!slot.HasAssociatedChildRunId (runtimeMessage.RunId))
                    return;

                IRuntimeMessage message = null;

                if (slot.PushGatheredResult(runtimeMessage.Result, ref message))
                {
                    _gatheringSlots.TryRemove(runtimeMessage.ContextId.Value, out slot);
                    slot.Dispose();
                }

                if(message != null)
                    _enqueueRuntimeMessage(message);
            }
        }

        public void Observe(FlowFailedMessage runtimeMessage)
        {
            if(!runtimeMessage.ContextId.HasValue)
                return;

            GatheringSlot slot;

            if (_gatheringSlots.TryGetValue(runtimeMessage.ContextId.Value, out slot))//if not this scatter has most probably been cancelled
            {
                if (!slot.HasAssociatedChildRunId (runtimeMessage.RunId))
                    return;

                IRuntimeMessage message = null;

                if (slot.PushGatheredException(runtimeMessage.CorrelationId, runtimeMessage.Exception,
                    runtimeMessage.CausativeInput, ref message))
                {
                    _gatheringSlots.TryRemove(runtimeMessage.ContextId.Value, out slot);
                    slot.Dispose();
                }

                if (message != null)
                    _enqueueRuntimeMessage(message);
            }
        }

        public void Observe(FlowCancelledMessage runtimeMessage)
        {
            if (!runtimeMessage.ContextId.HasValue)
                return;

            GatheringSlot slot;

            if (_gatheringSlots.TryGetValue(runtimeMessage.ContextId.Value, out slot))//if not this scatter has most probably been cancelled
            {
                if (!slot.HasAssociatedChildRunId (runtimeMessage.RunId))
                    return;

                IRuntimeMessage message = null;

                if (slot.PushGatheredCancellation(ref message))
                {
                    _gatheringSlots.TryRemove(runtimeMessage.ContextId.Value, out slot);
                    slot.Dispose();
                }

                if (message != null)
                    _enqueueRuntimeMessage(message);
            }
        }

        public void Clear()
        {
            //foreach(var slot in _gatheringSlots.Values)
            //{
            //    slot.Dispose ();
            //}

            _gatheringSlots.Clear ();
        }

        private GatheringSlot ReserveSlot(Guid contextId, Guid? previousContextId, int numberOfItemsToGather, RunId runIdToContinueOnAfterGathering, Guid originatingCorrelationId, Guid nextCorrelationId, Type gatheredResultType, bool ignoreExceptions, CancellationTokenSource originalCts, CancellationTokenSource scatterCts, CancellationTokenSource linkedCts)
        {
            var result = new GatheringSlot (numberOfItemsToGather, previousContextId, runIdToContinueOnAfterGathering, originatingCorrelationId, nextCorrelationId, gatheredResultType, ignoreExceptions, originalCts, scatterCts, linkedCts);
            _gatheringSlots.TryAdd(contextId, result);
            return result;
        }
    }
}