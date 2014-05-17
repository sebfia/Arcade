using System;

namespace Arcade.Dsl.Implementation
{
    public sealed class WriteStateFlowConfigurer<T> : BaseFlowConfigurer, IFlowConfigurer<T>, IWriteStateFlowConfigurer
    {
        private readonly Delegate _selectState;
        private readonly Delegate _selectOutput;

        public WriteStateFlowConfigurer(string flowName, IFlowConfigurer previous, Delegate selectState, Delegate selectOutput)
            : base(flowName, previous)
        {
            _selectState = selectState;
            _selectOutput = selectOutput;
        }

        public Delegate SelectState
        {
            get { return _selectState; }
        }

        public Delegate SelectOutput
        {
            get { return _selectOutput; }
        }
    }
}