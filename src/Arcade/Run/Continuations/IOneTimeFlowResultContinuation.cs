using Arcade.Run.Execution;

namespace Arcade.Run.Continuations
{
    public interface IOneTimeFlowResultContinuation : IFlowResultContinuation
    {
        RunId AffectedRunId { get; }
        bool ShouldDisposeCancellationTokenSource { get; }
    }
}