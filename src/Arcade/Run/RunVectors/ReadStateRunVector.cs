using System;
using Arcade.Dsl;
using Arcade.Run.Execution;

namespace Arcade.Run.RunVectors
{
    public sealed class ReadStateRunVector : IExecutableRunVector
    {
        private readonly Guid _correlationId;
        private readonly Guid _nextCorrelationId;
        private readonly Type _stateType;
        private readonly Delegate _resultCombinator;

        public ReadStateRunVector(Guid correlationId, Guid nextCorrelationId, Type stateType, Delegate resultCombinator)
        {
            _correlationId = correlationId;
            _nextCorrelationId = nextCorrelationId;
            _stateType = stateType;
            _resultCombinator = resultCombinator;
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
            var stateStore = executePackage.StateStore;
            var runId = executePackage.ExecuteMessage.RunId;
            var parameter = executePackage.ExecuteMessage.Parameter;

            Result state;

            if (!stateStore.TryReadState (runId, _stateType, out state)) 
            {
                whenFinished (new Tuple<Result, Exception>(null, new InvalidOperationException ("Unable to read state for type: " + _stateType.FullName + " ! Check your flow configuration for further details.")));
                return;
            }

            try 
            {
                var result = _resultCombinator.DynamicInvoke (new [] { parameter.Value, state.Value });
                whenFinished (new Tuple<Result, Exception>(new Result (result), null));
            } 
            catch (Exception ex) 
            {
                whenFinished (new Tuple<Result, Exception>(null, ex));
            }
        }

        public override string ToString()
        {
            return String.Format("Reading State of type: {0}", _stateType.Name);
        }
    }
}