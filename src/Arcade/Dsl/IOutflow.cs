using System;

namespace Arcade.Dsl
{
    public interface IOutflow<out TOut>
    {
        void Process();

        event Action<TOut> Result;
    }
}