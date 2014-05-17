using System;
using Arcade.Dsl;
using Arcade.Run.Execution;

namespace Arcade.Run.RunVectors
{
    /// <summary>
    /// A run vector that marks a condition in a flow.
    /// </summary>
    public sealed class ConditionalRunVector : IExecutableRunVector
    {
        private readonly Guid _correlationId;
        private Guid _nextCorrelationId;
        private readonly Conditional _conditional;

        public ConditionalRunVector(Guid correlationId, Conditional conditional)
        {
            _correlationId = correlationId;
            _conditional = conditional;
        }

        public void Run(ExecutePackage executePackage, Action<Tuple<Result, Exception>> whenFinished)
        {
            var input = executePackage.ExecuteMessage.Parameter;
            try
            {
                _nextCorrelationId = _conditional.NextCorrelationId (input);
                whenFinished (new Tuple<Result, Exception>(input, null));
            }
            catch(Exception ex)
            {
                whenFinished (new Tuple<Result, Exception>(null, ex));
            }

        }

        public Guid CorrelationId
        {
            get { return _correlationId; }
        }

        public Guid NextCorrelationId
        {
            get
            {
                return _nextCorrelationId;
            }
        }

        public TimeSpan Timeout
        {
            get { return 3.Seconds(); }
        }

        public override string ToString()
        {
            return "Evaluating condition";
        }
    }
}