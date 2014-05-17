using System;
using System.Reflection;
using System.Threading.Tasks;
using Arcade.Run.Execution.Messages;
using Arcade.Run.RunVectors;

namespace Arcade.Run.Execution.Pre
{
    public sealed class ExecutableRunVectorExecutionStrategy : IRunVectorExecutionStrategy
    {
        public bool CanExecutePackage(ExecutePackage executePackage)
        {
            return (executePackage.RunVector is IExecutableRunVector);
        }

        public void ExecutePackage(ExecutePackage executePackage, Action<IRunVectorExecutedMessage> continueWith)
        {
            var runVector = executePackage.RunVector as IExecutableRunVector;

            runVector.Run (executePackage, outcome => 
            {
                var runId = executePackage.ExecuteMessage.RunId;
                var correlationId = executePackage.ExecuteMessage.CorrelationId;
                var nextCorrelationId = runVector.NextCorrelationId;
                var causativeInput = executePackage.ExecuteMessage.Parameter.Value;
                var timeout = runVector.Timeout;

                var message = TransformToMessage(runId, correlationId, nextCorrelationId, causativeInput, timeout, outcome);

                continueWith(message);
            });
        }

        private static IRunVectorExecutedMessage TransformToMessage(RunId runId, Guid correlationId, Guid nextCorrelationId, object causativeInput, TimeSpan timeout, Tuple<Result, Exception> outcome)
        {
            var result = outcome.Item1;
            var exception = outcome.Item2;

            if (exception == null) {
                return new SuccessfulRunVectorExecutedMessage (runId, correlationId, nextCorrelationId, result);
            }

            if (exception is TimeoutException) {
                return new TimeoutRunVectorExecutedMessage (runId, correlationId, timeout, causativeInput);
            }

            if (exception is TaskCanceledException) {
                return new CancelledRunVectorExecutedMessage(runId, correlationId);
            }

            if (exception is TargetInvocationException) {
                return new ExceptionRunVectorExecutedMessage (runId, correlationId, (exception.InnerException ?? exception), causativeInput);
            }

            if (exception is ArgumentException) {
                return new ExceptionRunVectorExecutedMessage (runId, correlationId, 
                                                              new InputTypeNotMatchingOutputTypeException (correlationId, "Unable to execute flow due to an input argument mismatch! Check your flow configuration for branches where joined parameters do not match the required input type.", exception), 
                                                              causativeInput);
            }
            
            return new ExceptionRunVectorExecutedMessage (runId, correlationId, exception, causativeInput);
        }
    }
}