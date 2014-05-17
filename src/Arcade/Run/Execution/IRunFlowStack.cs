using System;
using Arcade.Run.Execution.Events;

namespace Arcade.Run.Execution
{
    /// <summary>
    /// Runs a flow stack after is has been transformed.
    /// </summary>
    public interface IRunFlowStack : IDisposable
    {
        void RunFlowStack(Result parameter);
        void RunFlowStackFromCorrelationId(Guid correlationId, Result parameter);
        event Action<IRunFlowStackEvent> ProgressPort;
    }
}