using System;

namespace Arcade.Build.FlowStack
{
    public class BuildFlowStackException : Exception
    {   
        public BuildFlowStackException (string message, Exception inner) : base (message, inner)
        {
        }
    }
    
}