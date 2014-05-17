using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Arcade.Run.Continuations;
using Arcade.Run.Messages;

namespace Arcade.Run.Observers
{
    public sealed class PortContinuationObserver : 
        IRuntimeMessageObserver<WaitOnPortMessage>,
        IRuntimeMessageObserver<PortResultMessage>
    {
        private readonly Action<IRuntimeMessage> _enqueueRuntimeMessage;
        private readonly ConcurrentDictionary<string, List<IPortContinuation>> _portContinuations;

        public PortContinuationObserver(Action<IRuntimeMessage> enqueueRuntimeMessage)
        {
            _enqueueRuntimeMessage = enqueueRuntimeMessage;
            _portContinuations = new ConcurrentDictionary<string, List<IPortContinuation>>();
        }

        public void Observe(WaitOnPortMessage runtimeMessage)
        {
            var key = runtimeMessage.PortName;
            var runId = runtimeMessage.RunId;

            IPortContinuation continuation = null;

            List<IPortContinuation> continuations;

            if (_portContinuations.TryGetValue(key, out continuations))
            {
                continuation = continuations.FirstOrDefault(x => ((x.FlowName == runId.ToString())));
            }

            if(continuation == null)
                throw new InvalidOperationException("No port has been created yet for flow " + runId + " although this is necessary to run this flow!");

            continuation.InvokePort(runtimeMessage);
        }

        public void Observe(PortResultMessage runtimeMessage)
        {
            var runId = runtimeMessage.RunId;
            var parameter = runtimeMessage.Result;
            var correlationIdToContinueFrom = runtimeMessage.CorrelationIdToContinueWith;
            var cts = runtimeMessage.CancellationTokenSource;
            var contextId = runtimeMessage.ContextId;

            _enqueueRuntimeMessage(new ContinueCompletedFlowMessage(runId, parameter, correlationIdToContinueFrom, cts, contextId));
        }

        public void AddPortContinuation(IPortContinuation portContinuation)
        {
            _portContinuations.AddOrUpdate(portContinuation.PortName,
                                           x => new List<IPortContinuation> { portContinuation },
                                           (y, oldList) =>
                                               {
                                                   var newList = new List<IPortContinuation> { portContinuation };
                                                   oldList.ForEach(newList.Add);
                                                   return newList;
                                               });
        }

        public void RemovePortContinuation(string portName, Guid portId)
        {
            _portContinuations.AddOrUpdate(portName, str => new List<IPortContinuation>(), 
                                           (str, oldList) => oldList.Where(item => item.PortId != portId).ToList());

        }
    }
}