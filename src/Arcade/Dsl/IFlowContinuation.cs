using System;

namespace Arcade.Dsl
{
    public interface IFlowContinuation<in TIn, out TOut>
    {
        void Process(TIn input, Action<TOut> continueWith);
    }
}