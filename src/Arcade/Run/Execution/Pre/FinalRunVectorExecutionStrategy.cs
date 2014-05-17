using System;
using Arcade.Run.Execution.Messages;
using Arcade.Run.RunVectors;

namespace Arcade.Run.Execution.Pre
{
    public sealed class FinalRunVectorExecutionStrategy : IRunVectorExecutionStrategy
    {
        public bool CanExecutePackage(ExecutePackage executePackage)
        {
            return executePackage.RunVector is FinalRunVector;
        }

        public void ExecutePackage(ExecutePackage executePackage, Action<IRunVectorExecutedMessage> continueWith)
        {
            var runVector = executePackage.RunVector;
            var correlationId = runVector.CorrelationId;
            var result = executePackage.ExecuteMessage.Parameter;
            var runId = executePackage.ExecuteMessage.RunId;

            continueWith(new FinalRunVectorExecutedMessage(runId, correlationId, result));
        }
    }
}