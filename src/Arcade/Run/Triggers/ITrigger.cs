using System;

namespace Arcade.Run.Triggers
{
    public interface ITrigger<out T> : IDisposable
    {
        event Action<T> When;
    }
}