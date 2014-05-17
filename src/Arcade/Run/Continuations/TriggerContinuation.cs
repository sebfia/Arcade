using System;
using Arcade.Run.Execution;

namespace Arcade.Run.Continuations
{
    public sealed class TriggerContinuation<T> : ITriggerContinuation
    {
        private readonly string _flowName;
        private readonly string _triggerName;
        private readonly Guid _triggerId;
        private readonly Action<T> _continuation;

        public TriggerContinuation(string flowName, string triggerName, Guid triggerId, Action<T> continueWith)
        {
            _flowName = flowName;
            _triggerName = triggerName;
            _triggerId = triggerId;
            _continuation = continueWith;
        }

        public void InvokeWithResult(Result result)
        {
            if (typeof(T) != result.Type)
                throw new InvalidOperationException("Unable to continue on trigger " + _triggerName + " due to type mismatch!");

            _continuation(result.Unbox<T>());
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