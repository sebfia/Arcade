using System;
using Arcade.Run.Execution;

namespace Arcade.Run.Continuations
{
    public sealed class FlowResultContinuation : IFlowResultContinuation
    {
        private readonly string _flowName;
        private readonly Action _continuation;
		private readonly Action<Exception> _onException;
        
		public FlowResultContinuation(string flowName, Action continuation, Action<Exception> onException)
        {
            _flowName = flowName;
            _continuation = continuation;
			_onException = onException;
        }

        public void ContinueWithResult(Result result)
        {
            if(result != Result.Empty)
                throw new InvalidOperationException("Unable to continue after flow completion due to type mismatch!");
            
            _continuation();
        }

        public void ContinueOnError(Exception exception)
        {
			_onException (exception);
        }

        public void ContinueWhenCancelled()
        {
			_onException (new OperationCanceledException ());
        }

        public string FlowName
        {
            get { return _flowName; }
        }
    }

    public sealed class FlowResultContinuation<T> : IFlowResultContinuation
    {
        private readonly string _flowName;
        private readonly Action<T> _continuation;
		private readonly Action<Exception> _onException;
        
        public FlowResultContinuation(string flowName, Action<T> continuation, Action<Exception> onException)
        {
            _flowName = flowName;
            _continuation = continuation;
			_onException = onException;
        }

        public void ContinueWithResult(Result result)
        {
            if (result.Type != typeof(T))
            {
                if (result.Type.IsAssignableFrom(typeof(T)))
                    throw new InvalidOperationException("Unable to continue after flow completion due to type mismatch!");
            }

            _continuation(result.Unbox<T>());
        }

        public void ContinueOnError(Exception exception)
        {
			_onException (exception);
        }

        public void ContinueWhenCancelled()
        {
			_onException (new OperationCanceledException ());
        }

        public string FlowName
        {
            get { return _flowName; }
        }
    }
}