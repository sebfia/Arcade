using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Arcade.Run.Execution;
using Arcade.Run.Execution.Messages;

namespace Arcade.Run.Aspects
{
    public sealed class TraceExceptionPostExecutionAdvice : IPostExecutionAdvice, ICanBeDeactivated
    {
        private readonly ThreadLocal<StringBuilder> _stringBuilder;
		private readonly Action<string> _log;
		private volatile bool _isActivated;

		public TraceExceptionPostExecutionAdvice(Action<string> log)
        {
			if (log == null)
				throw new ArgumentNullException ("log");

			_log = log;
            _stringBuilder = new ThreadLocal<StringBuilder>(() => new StringBuilder());
			_isActivated = true;
        }

		public void Activate ()
		{
			_isActivated = true;
		}

		public void Deactivate ()
		{
			_isActivated = false;
		}

        public bool Handles(IRunVectorExecutedMessage executedMessage)
        {
			if(_isActivated)
				return (executedMessage is ExceptionRunVectorExecutedMessage) ||
					(executedMessage is TimeoutRunVectorExecutedMessage);

            return false;
		}

        public void Handle(IRunVectorExecutedMessage executedMessage)
        {
			if (!_isActivated)
				return;

            if (executedMessage is ExceptionRunVectorExecutedMessage)
            {
                var message = (ExceptionRunVectorExecutedMessage) executedMessage;
                var flowName = GetFullFlowNameFromRunId(message.RunId);
                var correlationId = message.CorrelationId.ToString();
                var exceptionString = message.Exception.ToString();
                var causativeInput = message.CausativeInput.ToString();

                _stringBuilder.Value.AppendFormat("EXCEPTION while executing flow: {0} with input: {1} at CorrelationId: {2}{3}{4}",
                                                  flowName, causativeInput, correlationId, Environment.NewLine, exceptionString);
            }

            if (executedMessage is TimeoutRunVectorExecutedMessage)
            {
                var message = (TimeoutRunVectorExecutedMessage) executedMessage;
                var flowName = GetFullFlowNameFromRunId(message.RunId);
                var correlationId = message.CorrelationId.ToString();
                var timeoutInSeconds = message.Timeout.TotalSeconds;
                var causativeInput = message.CausativeInput.ToString();

                _stringBuilder.Value.AppendFormat("TIMEOUT of {0} sec. exceeded while executing flow: {1} with input: {2} at CorrelationId: {3}",
                                                  timeoutInSeconds, flowName, causativeInput, correlationId);
            }
            
			_log(_stringBuilder.Value.ToString());

            _stringBuilder.Value.Clear();
        }

        private string GetFullFlowNameFromRunId(RunId runId)
        {
            var linkedList = new LinkedList<string>();

            var current = runId;

            linkedList.AddLast(current.ToString());

            while (current.HasParent)
            {
                current = current.GetParent();
                linkedList.AddFirst(current.ToString());
            }

            _stringBuilder.Value.Clear();

            var currentNode = linkedList.First;

            _stringBuilder.Value.Append(currentNode.Value);

            while (currentNode.Next != null)
            {
                currentNode = currentNode.Next;
                _stringBuilder.Value.AppendFormat("/{0}", currentNode.Value);
            }

            var result = _stringBuilder.Value.ToString();

            _stringBuilder.Value.Clear();

            return result;
        }
    }
}