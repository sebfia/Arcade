using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Arcade.Run.Continuations;
using Arcade.Run.Messages;

namespace Arcade.Run.Observers
{
    public sealed class TriggerContinuationObserver : IRuntimeMessageObserver<TriggerMessage>
    {
        private readonly ConcurrentDictionary<string, List<ITriggerContinuation>> _triggerContinuations;

        public TriggerContinuationObserver()
        {
            _triggerContinuations = new ConcurrentDictionary<string, List<ITriggerContinuation>>();
        }

        public void Observe(TriggerMessage runtimeMessage)
        {
            var key = runtimeMessage.TriggerName;
            var result = runtimeMessage.Result;

            List<ITriggerContinuation> continuations;

            if (_triggerContinuations.TryGetValue(key, out continuations))
            {
                var list =
                    continuations.Where(x => ((x.FlowName == runtimeMessage.RunId) || (x.FlowName == "*"))).ToList();
                list.ForEach(continuation => continuation.InvokeWithResult(result));
            }
        }

        public void AddTriggerContinuation(ITriggerContinuation triggerContinuation)
        {
            _triggerContinuations.AddOrUpdate(triggerContinuation.TriggerName,
                                              x => new List<ITriggerContinuation> {triggerContinuation},
                                              (y, oldList) =>
                                                  {
                                                      var newList = new List<ITriggerContinuation> {triggerContinuation};
                                                      oldList.ForEach(newList.Add);
                                                      return newList;
                                                  });
        }

        public void RemoveTriggerContinuation(string triggerName, Guid triggerId)
        {
            _triggerContinuations.AddOrUpdate(triggerName, str => new List<ITriggerContinuation>(),
                                              (str, oldList) => oldList.Where(item => item.TriggerId != triggerId).ToList());
        }
    }
}