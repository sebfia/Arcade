using System;
using Arcade.Run.Messages;

namespace Arcade.Run.Continuations
{
    public sealed class PortContinuation<T> : IPortContinuation
    {
        private readonly string _flowName;
        private readonly string _portName;
        private readonly Action<WaitOnPortMessage> _continuation;
        private readonly Guid _portId;

        public PortContinuation(string flowName, string portName, Guid portId, Action<WaitOnPortMessage> continueWith)
        {
            _flowName = flowName;
            _portName = portName;
            _portId = portId;
            _continuation = continueWith;
        }

        public void InvokePort(WaitOnPortMessage waitOnPortMessage)
        {
            var result = waitOnPortMessage.Input;

            if (typeof(T) != result.Type)
                throw new InvalidOperationException("Unable to continue on port due to type mismatch!");

            _continuation(waitOnPortMessage);
        }

        public string FlowName
        {
            get { return _flowName; }
        }

        public string PortName
        {
            get { return _portName; }
        }

        public Guid PortId
        {
            get { return _portId; }
        }
    }
}