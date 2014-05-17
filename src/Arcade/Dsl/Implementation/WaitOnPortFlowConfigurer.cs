namespace Arcade.Dsl.Implementation
{
    public sealed class WaitOnPortFlowConfigurer<T> : BaseFlowConfigurer, IWaitOnPortFlowConfigurer, IFlowConfigurer<T>
    {
        private readonly string _portName;

        public WaitOnPortFlowConfigurer(string flowName, IFlowConfigurer previous, string portName)
            : base(flowName, previous)
        {
            _portName = portName;
        }

        public string PortName
        {
            get { return _portName; }
        }
    }
}