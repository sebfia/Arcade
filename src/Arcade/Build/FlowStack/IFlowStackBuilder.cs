using System;
using Arcade.Run.Execution;

namespace Arcade.Build.FlowStack
{
    public interface IFlowStackBuilder : IDisposable
    {
        IFlowStack BuildUpFlowStack();
    }
    
}