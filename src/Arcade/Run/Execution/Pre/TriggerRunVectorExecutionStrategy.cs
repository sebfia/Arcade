using System;
using Arcade.Run.Execution.Messages;
using Arcade.Run.RunVectors;

namespace Arcade.Run.Execution.Pre
{
    public sealed class TriggerRunVectorExecutionStrategy : IRunVectorExecutionStrategy
    {
        public bool CanExecutePackage(ExecutePackage executePackage)
        {
            return executePackage.RunVector is TriggerRunVector;
        }

        public void ExecutePackage(ExecutePackage executePackage, Action<IRunVectorExecutedMessage> continueWith)
        {
            var runVector = executePackage.RunVector as TriggerRunVector;

            if(runVector == null) throw new InvalidOperationException("Unable to execute this type of run vector!");

            var parameter = executePackage.ExecuteMessage.Parameter;
            var runId = executePackage.ExecuteMessage.RunId;
            var correlationId = runVector.CorrelationId;
            var nextCorrelationId = runVector.NextCorrelationId;
            var portName = runVector.TriggerName;
            var selectedResult = runVector.Select(executePackage.ExecuteMessage);

            continueWith(new TriggerRunVectorExecutedMessage(runId, correlationId, nextCorrelationId, portName, parameter, selectedResult));
        }
    }
}