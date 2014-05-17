using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Arcade.Run.Continuations;
using Arcade.Run.Execution;
using Arcade.Run.Messages;

namespace Arcade.Run.Observers
{
    /// <summary>
    /// This observer only reacts on completion of root flows.
    /// </summary>
    public sealed class FlowFinishedObserver : 
        IRuntimeMessageObserver<FlowCompleteMessage>,
        IRuntimeMessageObserver<FlowFailedMessage>,
        IRuntimeMessageObserver<FlowCancelledMessage>
    {
        private readonly ConcurrentDictionary<string, List<IFlowResultContinuation>> _resultContinuations;

        public FlowFinishedObserver()
        {
            _resultContinuations = new ConcurrentDictionary<string, List<IFlowResultContinuation>>();
        }

        public void AddFlowResultContinuation(IFlowResultContinuation flowResultContinuation)
        {
            var flowName = flowResultContinuation.FlowName;

            _resultContinuations.AddOrUpdate(flowName, 
            x => new List<IFlowResultContinuation>{ flowResultContinuation }, 
            (y, oldList) => {
                var newList = new List<IFlowResultContinuation>{ flowResultContinuation };
                oldList.ForEach(newList.Add);
                return newList;
            });
        }

        public void Observe(FlowCompleteMessage runtimeMessage)
        {
            if(runtimeMessage.RunId.HasParent)
                return;

            var runId = runtimeMessage.RunId;
            var flowName = runId.ToString();
            var result = runtimeMessage.Result;

            List<IFlowResultContinuation> continuations;

            if (_resultContinuations.TryGetValue(flowName, out continuations))
            {
                continuations.ForEach(continuation => continuation.ContinueWithResult(result));
                RemoveOneTimeCompletionQbserversForRunId(runId);
            }

            runtimeMessage.CancellationTokenSource.Dispose ();
        }

        public void Observe(FlowFailedMessage runtimeMessage)
        {
            if (runtimeMessage.RunId.HasParent)
                return;

            var runId = runtimeMessage.RunId;
            var flowName = runId.ToString();
            var exception = runtimeMessage.Exception;

            List<IFlowResultContinuation> continuations;

            if (_resultContinuations.TryGetValue(flowName, out continuations))
            {
                Debug.WriteLine("Found " + continuations.Count + " continuations.");
                continuations.ForEach(continuation => continuation.ContinueOnError(exception));
                RemoveOneTimeCompletionQbserversForRunId(runId);
            }

            runtimeMessage.CancellationTokenSource.Dispose ();
        }

        public void Observe(FlowCancelledMessage runtimeMessage)
        {
            if (runtimeMessage.RunId.HasParent)
                return;

            var runId = runtimeMessage.RunId;
            var flowName = runId.ToString();

            List<IFlowResultContinuation> continuations;

            if (_resultContinuations.TryGetValue(flowName, out continuations))
            {
                continuations.ForEach(continuation => continuation.ContinueWhenCancelled());
                RemoveOneTimeCompletionQbserversForRunId(runId);
            }

            runtimeMessage.CancellationTokenSource.Dispose ();
        }

        public void Clear()
        {
            _resultContinuations.Clear ();
        }

        private void RemoveOneTimeCompletionQbserversForRunId(RunId runId)
        {
            if (runId.HasParent)
                return;

            var flowName = runId.ToString();

            List<IFlowResultContinuation> continuations;

            if (_resultContinuations.TryGetValue(flowName, out continuations))
            {
                var toRemove = continuations.OfType<IOneTimeFlowResultContinuation>().Where(continuation => continuation.AffectedRunId == runId).Cast<IFlowResultContinuation>().ToList();

                if (toRemove.Count > 0)
                {
                    var updatedList = continuations.Where(continuation => !toRemove.Contains(continuation)).ToList();
                    UpdateContinuationsForFlowName(flowName, updatedList);
                }
            }
        }

        private void UpdateContinuationsForFlowName(string flowName, List<IFlowResultContinuation> continuations)
        {
            _resultContinuations.AddOrUpdate(flowName,
            x => continuations,
            (y, oldList) => continuations);
        }
    }
    
}