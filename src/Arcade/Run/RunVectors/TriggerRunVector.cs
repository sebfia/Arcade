using System;
using Arcade.Dsl;
using Arcade.Run.Execution;
using Arcade.Run.Execution.Messages;

namespace Arcade.Run.RunVectors
{
    public sealed class TriggerRunVector : IRunVector, ISelectableRunVector
    {
        private readonly Guid _correlationId;
        private readonly Guid _nextCorrelationId;
        private readonly string _triggerName;
        private readonly Delegate _selector;

        public TriggerRunVector(Guid correlationId, Guid nextCorrelationId, string triggerName, Delegate selector)
        {
            _correlationId = correlationId;
            _nextCorrelationId = nextCorrelationId;
            _triggerName = triggerName;
            _selector = selector;
        }

        public Guid CorrelationId
        {
            get { return _correlationId; }
        }

        public TimeSpan Timeout
        {
            get { return 1.Seconds(); }
        }

        public Guid NextCorrelationId
        {
            get { return _nextCorrelationId; }
        }

        public string TriggerName
        {
            get { return _triggerName; }
        }

        public Result Select(ExecuteRunVectorMessage message)
        {
            var input = message.Parameter.Value;
            var output = _selector.DynamicInvoke(new[] {input});
            return new Result(output);
        }

        public override string ToString()
        {
            return String.Format("Triggering: {0}", _triggerName);
        }
    }
}