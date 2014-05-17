using System;
using Arcade.Run.Execution;

namespace Arcade.Run.Continuations
{
    public interface ITriggerContinuation
    {
        void InvokeWithResult(Result result);
        string FlowName { get; }
        string TriggerName { get; }
        Guid TriggerId { get; }
    }
}