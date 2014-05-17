using System;
using Arcade.Run.RunVectors;

namespace Arcade.Run.Execution
{
    public interface IFlowStack
    {
        string FlowName { get; }
        IRunVector GetFirstRunVector();
        IRunVector GetRunVectorForCorrelationId(Guid correlationId);
    }
}