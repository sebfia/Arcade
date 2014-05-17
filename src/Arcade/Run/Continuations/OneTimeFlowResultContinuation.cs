using System;
using System.Threading.Tasks;
using Arcade.Run.Execution;

namespace Arcade.Run.Continuations
{
    public sealed class OneTimeFlowResultContinuation<T> : IOneTimeFlowResultContinuation
    {
        private readonly string _flowName;
        private readonly RunId _runId;
        private readonly TaskCompletionSource<T> _tcs;
        private readonly bool _shouldDisposeCancellationTokenSource;

        public OneTimeFlowResultContinuation(string flowName, RunId affectedRunId, TaskCompletionSource<T> tcs, bool shouldDisposeCancellationTokenSource)
        {
            _flowName = flowName;
            _runId = affectedRunId;
            _tcs = tcs;
            _shouldDisposeCancellationTokenSource = shouldDisposeCancellationTokenSource;
        }

        public void ContinueWithResult(Result result)
        {
            if (result.Type != typeof(T))
            {
                if (result.Type.IsAssignableFrom(typeof(T)))
                    throw new InvalidOperationException("Unable to continue after flow completion due to type mismatch!");
            }

            _tcs.TrySetResult(result.Unbox<T>());
        }

        public void ContinueOnError(Exception exception)
        {
            _tcs.TrySetException(exception);
        }

        public void ContinueWhenCancelled()
        {
            _tcs.TrySetCanceled();
        }

        public string FlowName
        {
            get { return _flowName; }
        }

        public RunId AffectedRunId 
        {
            get { return _runId; }
        }

        public bool ShouldDisposeCancellationTokenSource
        {
            get { return _shouldDisposeCancellationTokenSource; }
        }
    }
}