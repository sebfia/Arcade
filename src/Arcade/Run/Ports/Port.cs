using System;
using System.Collections.Concurrent;
using Arcade.Run.Execution;
using Arcade.Run.Messages;

namespace Arcade.Run.Ports
{
    public sealed class Port<TIn, TOut> : IPort<TIn, TOut>
    {
        private Action<PortResultMessage> _then;
        private readonly string _name;
        private readonly Guid _id;
        private Action<string, Guid> _remove;
        private Action<TIn, Action<TOut>> _interactor;
        private readonly ConcurrentStack<WaitOnPortMessage> _messagesWaitedOn;

        public Port(string name, Guid portId, Action<PortResultMessage> then, Action<string, Guid> removeFromObserver)
        {
            if (removeFromObserver == null)
                throw new ArgumentNullException("removeFromObserver");
            if (then == null) throw new ArgumentNullException("then");

            _name = name;
            _id = portId;
            _then = then;
            _remove = removeFromObserver;
            _messagesWaitedOn = new ConcurrentStack<WaitOnPortMessage>();
        }

        public void InvokePort(WaitOnPortMessage waitOnPortMessage)
        {
            if(_interactor == null) throw new InvalidOperationException("Port " + _name + " has been invoked without having an interactor assigned!");

            _messagesWaitedOn.Push(waitOnPortMessage);

            var input = waitOnPortMessage.Input;

            _interactor(input.Unbox<TIn>(), WhenCompleted);
        }

        public void WhenCompleted(TOut output)
        {
            WaitOnPortMessage waitOnPortMessage;

            if(!_messagesWaitedOn.TryPop(out waitOnPortMessage))
                throw new InvalidOperationException("Port " + _name + " has been interacted with, although there was no input to it!");

            var result = Result.FromValue(output);

            _then(new PortResultMessage(waitOnPortMessage.RunId, waitOnPortMessage.CorrelationIdToContinueWith, result, waitOnPortMessage.CancellationTokenSource, waitOnPortMessage.ContextId));
        }

        public void Dispose()
        {
            _messagesWaitedOn.Clear();
            _then = null;
            _remove(_name, _id);

            _remove = null;
        }

        public void AssignActor(Action<TIn, Action<TOut>> interactor)
        {
            _interactor = interactor;
        }
    }
}