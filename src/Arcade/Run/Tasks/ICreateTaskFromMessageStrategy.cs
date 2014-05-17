using System.Collections.Generic;
using System.Threading;
using Arcade.Build.FlowStack;
using Arcade.Run.Aspects;
using Arcade.Run.Execution;
using Arcade.Run.Messages;

namespace Arcade.Run.Tasks
{
    /// <summary>
    /// A kind of factory for creating tasks that is embedded in a chain of responsibility in the runtime engine.
    /// </summary>
    public interface ICreateTaskFromMessageStrategy
    {
        bool CanCreateTaskFromMessage(IRuntimeMessage runtimeMessage);
        ITask CreateTaskFromMessage(IRuntimeMessage runtimeMessage, IFlowStackBuilderFactory flowStackBuilderFactory, IStateStore stateStore, IEnumerable<IAspect> aspects, CancellationToken cancellationToken);
    }
}