using System;
using Arcade.Run.Execution;

namespace Arcade.Run.RunVectors
{
    public sealed class FunctionRunVector : IExecutableRunVector
    {
        private readonly Guid _correlationId;
        private readonly TimeSpan _timeout;
        private readonly Guid _nextCorrelationId;
        private readonly Delegate _function;

        public FunctionRunVector(Guid correlationId, Guid nextCorrelationId, Delegate function, TimeSpan timeout)
        {
            _correlationId = correlationId;
            _timeout = timeout;
            _nextCorrelationId = nextCorrelationId;
            _function = function;
        }

        public Guid CorrelationId
        {
            get { return _correlationId; }
        }

        public TimeSpan Timeout
        {
            get { return _timeout; }
        }

        public Guid NextCorrelationId
        {
            get { return _nextCorrelationId; }
        }

        public void Run(ExecutePackage executePackage, Action<Tuple<Result, Exception>> whenFinished)
        {
            var parameter = executePackage.ExecuteMessage.Parameter;

            try
            {
                var result = _function.DynamicInvoke (new [] { parameter.Value });
                whenFinished(new Tuple<Result, Exception>(new Result(result), null));
            }
            catch(Exception ex)
            {
                whenFinished (new Tuple<Result, Exception>(null, ex));
            }
        }

        public override string ToString()
        {
            return "Evaluating function";
        }
    }
    
}