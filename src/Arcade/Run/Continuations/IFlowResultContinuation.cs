using System;
using Arcade.Run.Execution;

namespace Arcade.Run.Continuations
{
    public interface IFlowResultContinuation
    {
        string FlowName { get; }
        void ContinueWithResult(Result result);
        void ContinueOnError(Exception exception);
        void ContinueWhenCancelled();
    }
}