using System;
using System.Threading;

namespace Arcade.Run.Tasks.Projections
{
    public interface ITaskVisitor
    {
        void Visit(ITask task);
        void SetCancellationTokenSource(CancellationTokenSource cts);
        void SetContextId(Guid? contextId);
    }
}