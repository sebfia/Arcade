using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Arcade.Build.RunVectors;
using Arcade.Dsl;
using Arcade.Run.Execution;

namespace Arcade.Build.FlowStack
{
    public sealed class FlowStackBuilderFactory : IFlowStackBuilderFactory
    {
        private readonly InstanceFactory _instanceFactory;
        private readonly IEnumerable<FlowConfiguration> _flowConfigurations;
        private readonly TimeSpan _standardTimeout;
        private readonly ThreadLocal<Dictionary<string, IFlowStack>> _threadLocalStackCache;
        
        public FlowStackBuilderFactory(InstanceFactory instanceFactory, IEnumerable<FlowConfiguration> flowConfigurations, TimeSpan standardTimeout)
        {
            _instanceFactory = instanceFactory;
            _flowConfigurations = flowConfigurations;
            _standardTimeout = standardTimeout;
            _threadLocalStackCache = new ThreadLocal<Dictionary<string, IFlowStack>>(() => new Dictionary<string, IFlowStack>());
        }

        public IFlowStackBuilder CreateFlowStackBuilder(string flowName)
        {
            if(_threadLocalStackCache.Value.ContainsKey(flowName))
                return new CachedFlowStackBuilder(flowName, _threadLocalStackCache.Value);

            return new SimpleFlowStackBuilder(flowName, _instanceFactory, _flowConfigurations, _standardTimeout, AddBuiltFlowStack);
        }

        public bool CanCreateFlowStackBuilderForFlowName(string flowName)
        {
            return _flowConfigurations.Any(x => x.Name == flowName);
        }

        private void AddBuiltFlowStack(IFlowStack flowStack)
        {
            _threadLocalStackCache.Value.Add(flowStack.FlowName, flowStack);
        }
    }
}