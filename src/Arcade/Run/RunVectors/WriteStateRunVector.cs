using System;
using Arcade.Dsl;
using Arcade.Run.Execution;

namespace Arcade.Run.RunVectors
{
    public sealed class WriteStateRunVector : IExecutableRunVector
    {
        private readonly Guid _correlationId;
        private readonly Guid _nextCorrelationId;
        private readonly Delegate _stateSelector;
        private readonly Delegate _resultSelector;

        public WriteStateRunVector(Guid correlationId, Guid nextCorrelationId, Delegate stateSelector, Delegate resultSelector = null)
        {
            _correlationId = correlationId;
            _nextCorrelationId = nextCorrelationId;
            _stateSelector = stateSelector;
            _resultSelector = resultSelector;
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

        public void Run(ExecutePackage executePackage, Action<Tuple<Result, Exception>> whenFinished)
        {
            var input = executePackage.ExecuteMessage.Parameter;
            var runId = executePackage.ExecuteMessage.RunId;
            var stateStore = executePackage.StateStore;

            try
            {
                var state = _stateSelector.DynamicInvoke(new [] { input.Value });
                var stateResult = new Result(state);
                stateStore.WriteState(runId, stateResult);

                var result = input;

                if(_resultSelector != null)
                {
                    var selected = _resultSelector.DynamicInvoke(new[] { input.Value });
                    result = new Result(selected);
                }

                whenFinished(new Tuple<Result, Exception>(result, null));
            }
            catch(Exception ex)
            {
                whenFinished (new Tuple<Result, Exception> (null, ex));
            }
        }

        public override string ToString()
        {
            return "Writing state";
        }
    }
}