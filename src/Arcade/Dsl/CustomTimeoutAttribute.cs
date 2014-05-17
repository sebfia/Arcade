using System;

namespace Arcade.Dsl
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CustomTimeoutAttribute : Attribute
    {
        public readonly int TimeoutInSeconds;

        public CustomTimeoutAttribute(int timeoutInSeconds)
        {
            TimeoutInSeconds = timeoutInSeconds;
        }
    }
}