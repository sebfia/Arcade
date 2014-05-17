using System;
using Arcade.Run.Execution;
using Arcade.Run.Messages;
using Arcade.Run.Tasks.Projections;

namespace Arcade.Run.Tasks
{
    /// <summary>
    /// The task to run when a runtime message is being dequeued in the runtime engine.
    /// </summary>
    public interface ITask
    {
        RunId RunId { get; }
        void Run(Action<IRuntimeMessage> observe);
        void AcceptVisitor(ITaskVisitor visitor);
    }
}