using System;
using System.Reflection;

namespace Arcade.Build.RunVectors
{
    public static class SimpleFindInputMethodStrategy
    {
        public static MethodInfo FindInputMethod(Type flowType)
        {
            return flowType.GetMethod("Process");
        }
    }
    
}