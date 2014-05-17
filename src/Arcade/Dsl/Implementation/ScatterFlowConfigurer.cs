namespace Arcade.Dsl.Implementation
{
    public sealed class ScatterFlowConfigurer<TOut> : BaseFlowConfigurer, IFlowConfigurer<TOut>, IScatterFlowConfigurer
    {
        private readonly IFlowConfigurer _start;
        private readonly IGatherFlowConfigurer _end;

        public ScatterFlowConfigurer(string flowName, IFlowConfigurer previous, IFlowConfigurer start, IGatherFlowConfigurer end)
            : base(flowName, previous)
        {
            _start = start;
            _end = end;
        }

        public IFlowConfigurer Start
        {
            get { return _start; }
        }

        public IGatherFlowConfigurer End
        {
            get { return _end; }
        }
    }
}