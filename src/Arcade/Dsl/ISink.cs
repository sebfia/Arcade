using System;

namespace Arcade.Dsl
{
    public interface ISink<in TIn>
    {
        void Process(TIn input);

        event Action Result;
    }
}