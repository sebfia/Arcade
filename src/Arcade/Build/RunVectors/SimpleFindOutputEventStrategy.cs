using System;
using System.Reflection;

namespace Arcade.Build.RunVectors
{

    public static class SimpleFindOutputEventStrategy
    {
        public static EventInfo FindOutputEvent(Type flowType)
        {
            return flowType.GetEvent("Result");
        }
    }
    
}