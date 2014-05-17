using System;

namespace Arcade.Dsl.Implementation
{
    public interface ISelfCreatingFlowConfigurer
    {
        Func<object> CreateInstance { get; }
    }
    
}