using System;
using Arcade.Run.Messages;

namespace Arcade.Run.Continuations
{
    public interface IPortContinuation
    {
        void InvokePort(WaitOnPortMessage waitOnPortMessage);
        string FlowName { get; }
        string PortName { get; }
        Guid PortId { get; }
    }
}