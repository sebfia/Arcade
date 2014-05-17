using System.Threading;
using Arcade.Run.Execution.Messages;
using Arcade.Run.RunVectors;

namespace Arcade.Run.Execution
{
    public sealed class ExecutePackage
    {
        public readonly IRunVector RunVector;
        public readonly IStateStore StateStore;
        public readonly CancellationToken CancellationToken;
        public readonly ExecuteRunVectorMessage ExecuteMessage;

        public ExecutePackage(IRunVector runVector, IStateStore stateStore, ExecuteRunVectorMessage executeMessage, CancellationToken cancellationToken)
        {
            RunVector = runVector;
            StateStore = stateStore;
            ExecuteMessage = executeMessage;
            CancellationToken = cancellationToken;
        }
    }
    
}