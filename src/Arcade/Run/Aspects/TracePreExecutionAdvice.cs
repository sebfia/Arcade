using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Arcade.Run.Execution;

namespace Arcade.Run.Aspects
{
    public class TracePreExecutionAdvice : IPreExecutionAdvice, ICanBeDeactivated
    {
        private readonly ThreadLocal<StringBuilder> _stringBuilder;
		private readonly Action<string> _log;
		private volatile bool _isActivated;

		public TracePreExecutionAdvice(Action<string> log)
        {
			if (log == null)
				throw new ArgumentNullException ("log");

			_log = log;
            _stringBuilder = new ThreadLocal<StringBuilder>(() => new StringBuilder());
			_isActivated = true;
        }

		public void Activate()
		{
			_isActivated = true;
		}

		public void Deactivate()
		{
			_isActivated = false;
		}

        public bool Handles(ExecutePackage executePackage)
        {
			return _isActivated;
        }

        public void Handle(ExecutePackage executePackage)
        {
			if (!_isActivated)
				return;

            try
            {
                if(executePackage == null)
                    return;

                _stringBuilder.Value.Clear();

                var flowName = GetFullFlowNameFromRunId(executePackage.ExecuteMessage.RunId);
                var correlationId = executePackage.ExecuteMessage.CorrelationId.ToString();
                var runVectorDescription = executePackage.RunVector.ToString();
                var runVectorTimeoutInSeconds = executePackage.RunVector.Timeout.TotalSeconds.ToString(CultureInfo.InvariantCulture);
                var inputDescription = (executePackage.ExecuteMessage.Parameter != null) ? executePackage.ExecuteMessage.Parameter.ToString() : "NULL";
                var newLine = Environment.NewLine;

                _stringBuilder.Value.AppendFormat("{0} <- {1}; Timeout: {2}sec.{3}on Flow: {4} and CorrelationId: {5}",
                                            runVectorDescription, inputDescription, runVectorTimeoutInSeconds, newLine,
                                            flowName, correlationId);

				_log(_stringBuilder.Value.ToString());

            }
            finally
            {
                _stringBuilder.Value.Clear();
            }
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