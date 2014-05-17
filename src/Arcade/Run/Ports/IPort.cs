using System;

namespace Arcade.Run.Ports
{
    public interface IPort<out TIn, in TOut> : IDisposable
    {
        void AssignActor(Action<TIn, Action<TOut>> interactor);
    }
}