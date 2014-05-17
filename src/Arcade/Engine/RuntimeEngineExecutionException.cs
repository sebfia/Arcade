using System;

namespace Arcade.Engine
{
    public class RuntimeEngineExecutionException : Exception
    {
        private readonly string _flowName;

        public RuntimeEngineExecutionException(string message)
            :base(message)
        {
        }

        public RuntimeEngineExecutionException(string message, Exception inner)
            : base(message, inner)
        {
            
        }

        public RuntimeEngineExecutionException(string flowName, string message)
            : this(flowName, message, null)
        {
        }

        public RuntimeEngineExecutionException(string flowName, string message, Exception inner)
            : base(message, inner)
        {
            _flowName = flowName;
        }

        public string AffectedFlowName
        {
            get { return _flowName; }
        }
    }
}