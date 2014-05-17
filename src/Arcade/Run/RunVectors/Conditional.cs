using System;
using Arcade.Run.Execution;

namespace Arcade.Run.RunVectors
{
    public sealed class Conditional
    {
        private readonly Guid _whenTrue;
        private readonly Guid _whenFalse;
        private readonly Delegate _evaluator;

        public Conditional(Delegate evaluator, Guid correlationIdWhenTrue, Guid correlationIdWhenFalse)
        {
            _evaluator = evaluator;
            _whenTrue = correlationIdWhenTrue;
            _whenFalse = correlationIdWhenFalse;
        }

        public Guid NextCorrelationId(Result previousResult)
        {
            if(previousResult == Result.Empty)
                throw new InvalidOperationException("Unable to reason on empty result!");

            object input = previousResult.Value;

            var result = (bool)_evaluator.DynamicInvoke(new[]{ input });

            return result ? _whenTrue : _whenFalse;
        }
    }
    
}