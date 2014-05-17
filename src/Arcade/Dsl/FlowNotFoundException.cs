using System;

namespace Arcade.Dsl
{
    public class FlowNotFoundException : Exception
    {
        public FlowNotFoundException(string flowName)
            : base(String.Format("No flow with name '{0}' was found!", flowName))
        {
        }
    }
    
}