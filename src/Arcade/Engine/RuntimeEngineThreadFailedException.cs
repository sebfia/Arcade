using System;
using Arcade.Run.Execution;

namespace Arcade.Engine
{
    public class RuntimeEngineThreadFailedException : Exception
    {
        private readonly RunId _runId;

        public RuntimeEngineThreadFailedException(RunId runId, Exception inner) : base("A runtime engine thread failed and had to be restarted!", inner)
        {
            _runId = runId;
        }

        public RunId RunId
        {
            get { return _runId; }
        }
    }
}