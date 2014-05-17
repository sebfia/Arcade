using System;

namespace Arcade.Run.RunVectors
{
    public sealed class WaitOnPortRunVector : IRunVector
    {
        private readonly Guid _correlationId;
        private readonly TimeSpan _timeout;
        private readonly Guid _nextCorrelationId;
        private readonly string _portName;

        public WaitOnPortRunVector(Guid correlationId, Guid nextCorrelationId, string portName, TimeSpan timeout)
        {
            _correlationId = correlationId;
            _nextCorrelationId = nextCorrelationId;
            _portName = portName;
            _timeout = timeout;
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

        public string PortName
        {
            get { return _portName; }
        }

        public override string ToString()
        {
            return String.Format("Waiting on port: {0}", _portName);
        }
    }
}