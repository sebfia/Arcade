using System;
using System.Threading;
using Arcade.Run.Execution.Events;
using Arcade.Run.Messages;

namespace Arcade.Run.Tasks.Projections
{
    public interface IProjectionBuilder
    {
        IProjectionBuilder For<TEvent>(Func<TEvent, CancellationTokenSource, Guid?, IRuntimeMessage> projection) where TEvent : IRunFlowStackEvent;
        Func<IRunFlowStackEvent, IRuntimeMessage> Build(ITask task);
    }
}