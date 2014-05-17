using System;
using System.Threading;
using System.Threading.Tasks;
using Arcade.Run.Execution;

namespace Arcade.Run.Continuations
{
    public sealed class ScheduledFlowResultContinuation : IFlowResultContinuation
    {
        private readonly string _flowName;
        private readonly Action _continuation;
		private readonly Action<Exception> _onException;
        private readonly TaskScheduler _taskScheduler;
        
        public ScheduledFlowResultContinuation(string flowName, Action continuation, Action<Exception> onException, TaskScheduler taskScheduler)
        {
            _flowName = flowName;
            _continuation = continuation;
			_onException = onException;
            _taskScheduler = taskScheduler;
        }

        public void ContinueWithResult(Result result)
        {
            Task.Factory.StartNew(() =>
                                      {
                                          if (result != Result.Empty)
                                              throw new InvalidOperationException("Unable to continue after flow completion due to type mismatch!");
                                          _continuation();
                                      }, 
                                      CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }

        public void ContinueOnError(Exception exception)
        {
            Task.Factory.StartNew(() =>
                {
                    _onException(exception);
                },CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }

        public void ContinueWhenCancelled()
        {
			Task.Factory.StartNew(() =>
			                      {
				_onException(new OperationCanceledException());
			},CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }

        public string FlowName
        {
            get { return _flowName; }
        }
    }

    public sealed class ScheduledFlowResultContinuation<T> : IFlowResultContinuation
    {
        private readonly string _flowName;
        private readonly Action<T> _continuation;
		private readonly Action<Exception> _onException;
        private readonly TaskScheduler _taskScheduler;
        
        public ScheduledFlowResultContinuation(string flowName, Action<T> continuation, Action<Exception> onException, TaskScheduler taskScheduler)
        {
            _flowName = flowName;
            _continuation = continuation;
			_onException = onException;
            _taskScheduler = taskScheduler;
        }
        
        public void ContinueWithResult(Result result)
        {
            Task.Factory.StartNew(() =>
                                      {
                                          if(result.Type != typeof(T))
                                          {
                                          if (result.Type.IsAssignableFrom(typeof(T)))
                                              throw new InvalidOperationException("Unable to continue on port due to type mismatch!");
                                          }

                                          _continuation(result.Unbox<T>());
                                      }, 
                                      CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }

        public void ContinueOnError(Exception exception)
        {
            Task.Factory.StartNew(() =>
            {
                _onException(exception);
            }, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }

        public void ContinueWhenCancelled()
        {
			Task.Factory.StartNew(() =>
			                      {
				_onException(new OperationCanceledException());
			}, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }

        public string FlowName
        {
            get { return _flowName; }
        }
    }
}