using System;
using Arcade.Run.Execution.Messages;
using Arcade.Run.RunVectors;

namespace Arcade.Run.Execution.Pre
{
    public sealed class ScatterRunVectorExecutionStrategy : IRunVectorExecutionStrategy
    {
        public bool CanExecutePackage(ExecutePackage executePackage)
        {
            return executePackage.RunVector is ScatterRunVector;
        }

        public void ExecutePackage(ExecutePackage executePackage, Action<IRunVectorExecutedMessage> continueWith)
        {
            var runVector = executePackage.RunVector as ScatterRunVector;

            if(runVector == null) throw new InvalidOperationException("Unable to execute this type of run vector!");

            var parameter = executePackage.ExecuteMessage.Parameter;
            var runId = executePackage.ExecuteMessage.RunId;
            var correlationId = runVector.CorrelationId;
            var correlationIdToStartWith = runVector.CorrelationIdToStartWith;
            var nextCorrelationId = runVector.NextCorrelationId;
            var treatExceptions = runVector.TreatExceptions;
            var timeout = runVector.Timeout;
            var gatheredResultType = runVector.GatheredResultType;

            continueWith(new ScatterRunVectorExecutedMessage(runId, correlationId, parameter, correlationIdToStartWith, nextCorrelationId, gatheredResultType, treatExceptions, timeout));
        }
    }
}