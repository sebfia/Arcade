using System;
using System.Threading;
using System.Threading.Tasks;
using Arcade.Run.Execution;

namespace Arcade.Run.Continuations
{
    public sealed class ScheduledTriggerContinuation<T> : ITriggerContinuation
    {
        private readonly string _flowName;
        private readonly string _triggerName;
        private readonly Guid _triggerId;
        private readonly Action<T> _continuation;
        private readonly TaskScheduler _scheduler;

        public ScheduledTriggerContinuation(string flowName, string triggerName, Guid triggerId, Action<T> continueWith, TaskScheduler scheduler)
        {
            _flowName = flowName;
            _triggerName = triggerName;
            _triggerId = triggerId;
            _continuation = continueWith;
            _scheduler = scheduler;
        }

        public void InvokeWithResult(Result result)
        {
            Task.Factory.StartNew(() =>
                                      {
                                          if (typeof (T) != result.Type)
                                              throw new InvalidOperationException("Unable to continue on trigger: " +
                                                                                  _triggerName +
                                                                                  " due to type mismatch!");
                                          _continuation(result.Unbox<T>());
                                      }, CancellationToken.None, TaskCreationOptions.None, _scheduler);
        }

        public string FlowName
        {
            get { return _flowName; }
        }

        public string TriggerName
        {
            get { return _triggerName; }
        }

        public Guid TriggerId
        {
            get { return _triggerId; }
        }
    }
}