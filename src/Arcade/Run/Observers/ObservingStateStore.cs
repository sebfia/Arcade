using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Arcade.Run.Execution;
using Arcade.Run.Messages;

namespace Arcade.Run.Observers
{
    public sealed class ObservingStateStore : IStateStore, 
                                              IRuntimeMessageObserver<FlowCompleteMessage>,
                                              IRuntimeMessageObserver<FlowFailedMessage>,
                                              IRuntimeMessageObserver<FlowCancelledMessage>
    {
        private readonly ConcurrentDictionary<RunId, Dictionary<Type, Result>> _storedStates;

        public ObservingStateStore()
        {
            _storedStates = new ConcurrentDictionary<RunId, Dictionary<Type, Result>>();
        }

        public void WriteState(RunId runId, Result state)
        {
            _storedStates.AddOrUpdate(runId,
                                      id => NewRunIdLocalStateDictionary(state),
                                      (id, results) => UpdateRunIdLocalStateDictionary(results, state));
        }

        private static Dictionary<Type, Result> NewRunIdLocalStateDictionary(Result state)
        {
            return new Dictionary<Type, Result>
                       {
                           {state.Type, state}
                       };
        }

        private static Dictionary<Type, Result> UpdateRunIdLocalStateDictionary(Dictionary<Type, Result> oldStateDictionary, Result state)
        {
            var newDictionary = new Dictionary<Type, Result>(oldStateDictionary.Count + 1);

            foreach (var pair in oldStateDictionary)
            {
                newDictionary.Add(pair.Key, pair.Value);
            }

            if (newDictionary.ContainsKey(state.Type))
            {
                newDictionary.Remove(state.Type);
            }

            newDictionary.Add(state.Type, state);

            return newDictionary;
        }

        public bool TryReadState(RunId runId, Type stateType, out Result state)
        {
            state = null;
            Dictionary<Type, Result> states;

            if(_storedStates.TryGetValue(runId, out states))
            {
                if(!states.TryGetValue(stateType, out state))
                {
                    var firstKey = states.Keys.FirstOrDefault(stateType.IsAssignableFrom);

                    if(firstKey == null)
                        return false;

                    return states.TryGetValue(firstKey, out state);
                }

                return true;
            }

            return false;
        }

        public void Observe(FlowCompleteMessage runtimeMessage)
        {
            RemoveAllStatesForRunId(runtimeMessage.RunId);
        }

        public void Observe(FlowFailedMessage runtimeMessage)
        {
            RemoveAllStatesForRunId(runtimeMessage.RunId);
        }

        public void Observe(FlowCancelledMessage runtimeMessage)
        {
            RemoveAllStatesForRunId(runtimeMessage.RunId);
        }

        public void Clear()
        {
            _storedStates.Clear ();
        }

        private void RemoveAllStatesForRunId(RunId runId)
        {
            if (_storedStates.ContainsKey(runId))
            {
                Dictionary<Type, Result> states;
                if(!_storedStates.TryRemove(runId, out states))
                    throw new InvalidOperationException("Unable to remove all states for run id: " + runId);
            }
        }
    }
}