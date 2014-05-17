using System;
using Arcade.Dsl;
using Arcade.Run.Execution;
using Arcade.Run.Execution.Messages;

namespace Arcade.Run.RunVectors
{
    /// <summary>
    /// A run vector that marks the end of a flow.
    /// </summary>
    public sealed class FinalRunVector : IRunVector
    {
        private readonly Guid _correlationId;

        public FinalRunVector(Guid correlationId)
        {
            _correlationId = correlationId;
        }

        public void Run(ExecuteRunVectorMessage message, Action<Result> continuation)
        {

        }

        public Guid CorrelationId
        {
            get { return _correlationId; }
        }

        public Guid NextCorrelationId
        {
            get { return Guid.Empty; }
        }

        public TimeSpan Timeout
        {
            get { return 0.Seconds(); }
        }

        public override string ToString()
        {
            return "Finishing flow";
        }
    }
}