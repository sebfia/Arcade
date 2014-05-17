using System;
using Arcade.Run.Execution.Messages;
using Arcade.Run.RunVectors;

namespace Arcade.Run.Execution.Pre
{
    public sealed class ContinueWithNamedFlowRunVectorExecutionStrategy : IRunVectorExecutionStrategy
    {
        public bool CanExecutePackage(ExecutePackage executePackage)
        {
            return executePackage.RunVector is ContinueWithNamedFlowRunVector;
        }

        public void ExecutePackage(ExecutePackage executePackage, Action<IRunVectorExecutedMessage> continueWith)
        {
            var runVector = executePackage.RunVector as ContinueWithNamedFlowRunVector;

            if(runVector == null) throw new InvalidOperationException("Unable to execute this type of run vector!");

            var parameter = executePackage.ExecuteMessage.Parameter;
            var runId = executePackage.ExecuteMessage.RunId;
            var correlationId = runVector.CorrelationId;
            var nextCorrelationId = runVector.NextCorrelationId;
            var flowName = runVector.FlowName;

            continueWith(new ContinueWithNamedFlowRunVectorExecutedMessage(runId, correlationId, parameter, flowName, nextCorrelationId));
        }
    }
}