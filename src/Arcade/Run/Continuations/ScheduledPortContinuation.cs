using System;
using System.Threading;
using System.Threading.Tasks;
using Arcade.Run.Messages;

namespace Arcade.Run.Continuations
{
    public sealed class ScheduledPortContinuation<T> : IPortContinuation
    {
        private readonly string _flowName;
        private readonly string _portName;
        private readonly Action<WaitOnPortMessage> _continuation;
        private readonly TaskScheduler _scheduler;
        private readonly Guid _portId;

        public ScheduledPortContinuation(string flowName, string portName, Guid portId, Action<WaitOnPortMessage> continueWith, TaskScheduler scheduler)
        {
            _flowName = flowName;
            _portName = portName;
            _continuation = continueWith;
            _scheduler = scheduler;
            _portId = portId;
        }

        public void InvokePort(WaitOnPortMessage waitOnPortMessage)
        {
            Task.Factory.StartNew(() =>
                                      {
                                          var result = waitOnPortMessage.Input;

                                          if (typeof(T) != result.Type)
                                              throw new InvalidOperationException("Unable to continue on port due to type mismatch!");
                                          
                                          _continuation(waitOnPortMessage);
                                      }, 
                                      CancellationToken.None, TaskCreationOptions.None, _scheduler);
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