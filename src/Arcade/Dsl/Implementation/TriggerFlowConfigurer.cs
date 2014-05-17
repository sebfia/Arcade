using System;

namespace Arcade.Dsl.Implementation
{
    public sealed class TriggerFlowConfigurer<T> : BaseFlowConfigurer, ITriggerFlowConfigurer, IFlowConfigurer<T>
    {
        private readonly string _triggerName;
        private readonly Delegate _selector;

        public TriggerFlowConfigurer(string flowName, IFlowConfigurer previous, string triggerName, Delegate selector)
            : base(flowName, previous)
        {
            _triggerName = triggerName;
            _selector = selector;
        }

        public string TriggerName
        {
            get { return _triggerName; }
        }

        public Delegate Selector
        {
            get { return _selector; }
        }
    }
}