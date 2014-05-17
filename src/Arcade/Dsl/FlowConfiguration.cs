using System;
using Arcade.Dsl.Implementation;

namespace Arcade.Dsl
{
    public class FlowConfiguration
    {
        private readonly FinalFlowConfigurer _lastFlowConfigurer;

        public FlowConfiguration(FinalFlowConfigurer lastFlowConfigurer)
        {
            if(lastFlowConfigurer == null)
                throw new ArgumentNullException("lastFlowConfigurer", "Last flow configurer must not be null!");

            _lastFlowConfigurer = lastFlowConfigurer;
        }

        public FinalFlowConfigurer LastFlowConfigurer
        {
            get
            {
                return _lastFlowConfigurer;
            }
        }

        public string Name
        {
            get { return _lastFlowConfigurer.FlowName; }
        }
    }
}