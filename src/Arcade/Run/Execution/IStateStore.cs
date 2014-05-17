using System;

namespace Arcade.Run.Execution
{
    public interface IStateStore
    {
        void WriteState(RunId runId, Result state);
        bool TryReadState(RunId runId, Type stateType, out Result state);
    }
}