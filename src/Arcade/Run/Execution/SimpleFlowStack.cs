using System;
using System.Collections.Generic;
using Arcade.Run.RunVectors;

namespace Arcade.Run.Execution
{
    public sealed class SimpleFlowStack : IFlowStack
    {
        private readonly Guid _firstRunVectorCorrelationId;
        private readonly string _flowName;
        private readonly Dictionary<Guid, IRunVector> _runVecors;

        public SimpleFlowStack(string flowName, Guid firstRunVectorCorrelationId, IEnumerable<IRunVector> runVectors)
        {
            _flowName = flowName;
            _firstRunVectorCorrelationId = firstRunVectorCorrelationId;
            _runVecors = new Dictionary<Guid, IRunVector>();
            AddRunVectors(runVectors);
        }

        private void AddRunVectors(IEnumerable<IRunVector> runVectors)
        {
            foreach (var runVector in runVectors)
            {
                _runVecors.Add(runVector.CorrelationId, runVector);
            }
        }

        public string FlowName
        {
            get
            {
                return _flowName;
            }
        }

        public IRunVector GetFirstRunVector()
        {
            IRunVector result;

            if(!_runVecors.TryGetValue(_firstRunVectorCorrelationId, out result))
                throw new InvalidOperationException("This flow stack has no run vector!");

            return result;
        }

        public IRunVector GetRunVectorForCorrelationId(Guid correlationId)
        {
            IRunVector result;

            if(!_runVecors.TryGetValue(correlationId, out result))
                throw new InvalidOperationException("No run vector found for correlation id: " + correlationId.ToString() + " on flow stack for flow " + _flowName);

            return result;
        }
    }
}