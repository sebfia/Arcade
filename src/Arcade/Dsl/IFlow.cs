using System;

namespace Arcade.Dsl
{
    public interface IFlow<in TIn, out TOut>
    {
        void Process(TIn input);

        event Action<TOut> Result;
    }

    public interface IFlow
    {
        void Process();

        event Action Result;
    }
}