using System;

namespace Arcade.Dsl.Implementation
{
    public sealed class BranchEnd
    {
        private readonly IFlowConfigurer _last;
        private readonly string _joinpointName;

        public BranchEnd(IFlowConfigurer last, string joinpointName = null)
        {
            _last = last;
            _joinpointName = joinpointName ?? String.Empty;
        }

        public string JoinpointName
        {
            get { return _joinpointName; }
        }

        public IFlowConfigurer Last
        {
            get { return _last; }
        }
    }
    
}