using System;

namespace Arcade.Run.Execution
{
    public class InputTypeNotMatchingOutputTypeException : Exception
    {
        private readonly Guid _correlationId;

        public InputTypeNotMatchingOutputTypeException (Guid correlationId, string message, Exception inner) : base (message, inner)
        {
            _correlationId = correlationId;
        }

        public Guid AffectedCorrelationId
        {
            get { return _correlationId; }
        }
    }
    
}