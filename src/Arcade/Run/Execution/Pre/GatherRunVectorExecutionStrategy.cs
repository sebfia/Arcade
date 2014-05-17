using System;
using Arcade.Run.Execution.Messages;
using Arcade.Run.RunVectors;

namespace Arcade.Run.Execution.Pre
{
    public sealed class GatherRunVectorExecutionStrategy : IRunVectorExecutionStrategy
    {
        public bool CanExecutePackage(ExecutePackage executePackage)
        {
            return executePackage.RunVector is GatherRunVector;
        }

        public void ExecutePackage(ExecutePackage executePackage, Action<IRunVectorExecutedMessage> continueWith)
        {
            var runVector = executePackage.RunVector;
            var result = executePackage.ExecuteMessage.Parameter;
            var runId = executePackage.ExecuteMessage.RunId;
            var correlationId = runVector.CorrelationId;

            continueWith(new GatherRunVectorExecutedMessage(runId, correlationId, result));
        }
    }
}