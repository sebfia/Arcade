using System;

namespace Arcade.Run.Triggers
{
    public class Trigger<T> : ITrigger<T>
    {
        private readonly string _name;
        private readonly Guid _id;
        private Action<string, Guid> _remove;

        public Trigger(string name, Guid id, Action<string, Guid> remove)
        {
            _name = name;
            _id = id;
            _remove = remove;
        }

        public void Dispose()
        {
            _remove(_name, _id);
            var eventInfo = GetType().GetEvent("When");
            var invokers = When.GetInvocationList();

            foreach (var invoker in invokers)
            {
                eventInfo.RemoveEventHandler(this, invoker);
            }

            _remove = null;
        }

        public event Action<T> When;

        public void Invoke(T input)
        {
            if (When != null)
                When(input);
        }
    }
}