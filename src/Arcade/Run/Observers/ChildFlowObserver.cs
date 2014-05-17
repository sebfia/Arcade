using System;
using System.Collections.Concurrent;
using Arcade.Run.Execution;
using Arcade.Run.Messages;

namespace Arcade.Run.Observers
{
    public sealed class ChildFlowObserver :
        IRuntimeMessageObserver<InitializeChildFlowMessage>,
        IRuntimeMessageObserver<FlowCompleteMessage>,
        IRuntimeMessageObserver<FlowFailedMessage>,
        IRuntimeMessageObserver<FlowCancelledMessage>
    {
        private readonly Action<IRuntimeMessage> _enqueueRuntimeMessage;
        private readonly ConcurrentDictionary<RunId, Guid> _flowCorrelationHistory;

        public ChildFlowObserver(Action<IRuntimeMessage> enqueueRuntimeMessage)
        {
            _enqueueRuntimeMessage = enqueueRuntimeMessage;
            _flowCorrelationHistory = new ConcurrentDictionary<RunId, Guid>();
        }

        public void Observe(InitializeChildFlowMessage runtimeMessage)
        {
            var parentRunId = runtimeMessage.RunId;
            var parameter = runtimeMessage.Parameter;
            var childRunId = parentRunId.NewChild(runtimeMessage.ChildFlowName);
            var cts = runtimeMessage.CancellationTokenSource;
            var ctx = runtimeMessage.ContextId;

            _flowCorrelationHistory.TryAdd(childRunId, runtimeMessage.CorrelationIdToContinueWithOnParent);

            _enqueueRuntimeMessage(new RunFlowMessage(childRunId, parameter, cts, ctx));
        }

        public void Observe(FlowCompleteMessage runtimeMessage)
        {
            if (!runtimeMessage.RunId.HasParent)
                return;

//			System.Diagnostics.Debug.WriteLine ("CFObserver OBSERVING: " + runtimeMessage.Result.Value.ToString ());

			Guid correlationIdToContinueWith;

			if(_flowCorrelationHistory.TryRemove(runtimeMessage.RunId, out correlationIdToContinueWith))
			{
				var parentId = runtimeMessage.RunId.GetParent();
				var parameter = runtimeMessage.Result;
				var cts = runtimeMessage.CancellationTokenSource;
				var ctx = runtimeMessage.ContextId;

//				System.Diagnostics.Debug.WriteLine ("CFObserver ENQUEUING: " + parameter.Value.ToString ());

				_enqueueRuntimeMessage(new ContinueCompletedFlowMessage(parentId, parameter, correlationIdToContinueWith, cts, ctx));
			}
        }

        public void Observe(FlowFailedMessage runtimeMessage)
        {
            if (!runtimeMessage.RunId.HasParent)
                return;

			Guid correlationIdToContinueWith;

			if(_flowCorrelationHistory.TryRemove(runtimeMessage.RunId, out correlationIdToContinueWith))
			{
				var parentId = runtimeMessage.RunId.GetParent();
				var exception = runtimeMessage.Exception;
				var causativeInput = runtimeMessage.CausativeInput;
				var cts = runtimeMessage.CancellationTokenSource;
				var ctx = runtimeMessage.ContextId;

				_enqueueRuntimeMessage(new ContinueFailedFlowMessage(parentId, exception, causativeInput, correlationIdToContinueWith, cts, ctx));
			}
        }


        public void Observe(FlowCancelledMessage runtimeMessage)
        {
            if (!runtimeMessage.RunId.HasParent)
                return;
            
            Guid correlationIdToContinueWith;
            
			if(_flowCorrelationHistory.TryRemove(runtimeMessage.RunId, out correlationIdToContinueWith))
            {
				var parentId = runtimeMessage.RunId.GetParent();
				var cts = runtimeMessage.CancellationTokenSource;
				var ctx = runtimeMessage.ContextId;

                _enqueueRuntimeMessage(new ContinueCancelledFlowMessage(parentId, correlationIdToContinueWith, cts, ctx));
            }
        }

        public void Clear()
        {
            _flowCorrelationHistory.Clear ();
        }
    }
}