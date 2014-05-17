namespace Arcade.Dsl.Implementation
{
    public sealed class ContinueWithNamedFlowConfigurer : BaseFlowConfigurer, IContinueWithNamedFlowConfigurer
    {
        private readonly string _flowNameToContinueWith;

        public ContinueWithNamedFlowConfigurer(string flowName, IFlowConfigurer previous, string flowNameToContinueWith)
            : base(flowName, previous)
        {
            _flowNameToContinueWith = flowNameToContinueWith;
        }

        public string FlowNameToContinueWith
        { 
            get { return _flowNameToContinueWith; }
        }
    }

    public sealed class ContinueWithNamedFlowConfigurer<TOut> : BaseFlowConfigurer, IContinueWithNamedFlowConfigurer, IFlowConfigurer<TOut>
    {
        private readonly string _flowNameToContinueWith;
        
        public ContinueWithNamedFlowConfigurer(string flowName, IFlowConfigurer previous, string flowNameToContinueWith)
            : base(flowName, previous)
        {
            _flowNameToContinueWith = flowNameToContinueWith;
        }
        
        public string FlowNameToContinueWith
        { 
            get { return _flowNameToContinueWith; }
        }
    }
}