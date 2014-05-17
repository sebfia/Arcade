using System.Collections.Generic;
using Arcade.Dsl;
using Arcade.Run.Execution;

namespace Arcade.Build.FlowStack
{
    public sealed class CachedFlowStackBuilder : IFlowStackBuilder
    {
        private readonly string _flowName;
        private IDictionary<string, IFlowStack> _cache;

        public CachedFlowStackBuilder(string flowName, IDictionary<string, IFlowStack> flowStackCache)
        {
            _flowName = flowName;
            _cache = flowStackCache;
        }

        public void Dispose()
        {
            _cache = null;
        }

        public IFlowStack BuildUpFlowStack()
        {
            IFlowStack result;

            if (!_cache.TryGetValue(_flowName, out result))
            {
                throw new FlowNotFoundException(_flowName);
            }

            return result;
        }
    }
}