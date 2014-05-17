using System;
using System.Collections.Generic;
using System.Linq;

namespace Arcade.Engine
{
    public class RuntimeGatherException : AggregateException
    {
        public RuntimeGatherException(string message, IEnumerable<ScatterFlowFailedException> innerExceptions)
            : base(message, innerExceptions.Cast<Exception>())
        {
        }
    }
}