using System;
using Arcade.Run.Execution.Messages;
using Arcade.Run.RunVectors;

namespace Arcade.Run.Execution.Pre
{
    public sealed class WaitOnPortRunVectorExecutionStrategy : IRunVectorExecutionStrategy
    {
        public bool CanExecutePackage(ExecutePackage executePackage)
        {
            return executePackage.RunVector is WaitOnPortRunVector;
        }

        public void ExecutePackage(ExecutePackage executePackage, Action<IRunVectorExecutedMessage> continueWith)
        {
            var runVector = executePackage.RunVector as WaitOnPortRunVector;

            if(runVector == null) throw new InvalidOperationException("Unable to execute this type of run vector.");

            var parameter = executePackage.ExecuteMessage.Parameter;
            var runId = executePackage.ExecuteMessage.RunId;
            var correlationId = runVector.CorrelationId;
            var nextCorrelationId = runVector.NextCorrelationId;
            var timeout = runVector.Timeout;
            var portName = runVector.PortName;

            continueWith(new WaitOnPortRunVectorExecutedMessage(runId, correlationId, nextCorrelationId, portName, parameter, timeout));
        }
    }
}