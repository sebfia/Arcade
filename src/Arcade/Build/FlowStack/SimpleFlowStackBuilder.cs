using System;
using System.Collections.Generic;
using System.Linq;
using Arcade.Build.RunVectors;
using Arcade.Dsl;
using Arcade.Dsl.Implementation;
using Arcade.Run.Execution;
using Arcade.Run.RunVectors;

namespace Arcade.Build.FlowStack
{
    public sealed class SimpleFlowStackBuilder : IFlowStackBuilder, IStackBuilder
    {
        private readonly string _flowName;
        private readonly List<IRunVector> _runVectors;
        private readonly LinkedList<IRunVectorBuilder> _runVectorBuilders;
        private readonly Dictionary<string, Guid> _joinpointNameToCorrelationId;
        private readonly IEnumerable<FlowConfiguration> _flowConfigurations;
        private Action<IFlowStack> _whenBuilt;

        public SimpleFlowStackBuilder(string flowName, InstanceFactory instanceFactory, IEnumerable<FlowConfiguration> flowConfigurations, TimeSpan standardTimeout, Action<IFlowStack> whenBuilt)
        {
            _flowName = flowName;
            _flowConfigurations = flowConfigurations;
            _whenBuilt = whenBuilt;
            _runVectors = new List<IRunVector>();
            _joinpointNameToCorrelationId = new Dictionary<string, Guid>();
            _runVectorBuilders = new LinkedList<IRunVectorBuilder>();

            _runVectorBuilders.AddLast(new EbcRunVectorBuilder(instanceFactory, SimpleFindInputMethodStrategy.FindInputMethod, SimpleFindOutputEventStrategy.FindOutputEvent, standardTimeout));
            _runVectorBuilders.AddLast(new ContinuationRunVectorBuilder(instanceFactory, standardTimeout));
            _runVectorBuilders.AddLast(new FunctionRunVectorBuilder(standardTimeout));
            _runVectorBuilders.AddLast(new ConditionalRunVectorBuilder());
            _runVectorBuilders.AddLast(new WriteStateRunVectorBuilder());
            _runVectorBuilders.AddLast(new ReadStateRunVectorBuilder());
            _runVectorBuilders.AddLast(new ScatterRunVectorBuilder());
            _runVectorBuilders.AddLast(new GatherRunVectorBuilder());
            _runVectorBuilders.AddLast(new GoToRunVectorBuilder());
            _runVectorBuilders.AddLast(new TriggerRunVectorBuilder());
            _runVectorBuilders.AddLast(new WaitOnPortRunVectorBuilder());
            _runVectorBuilders.AddLast(new FinalRunVectorBuilder());
            _runVectorBuilders.AddLast(new JoinpointRunVectorBuilder());
            _runVectorBuilders.AddLast(new ContinueWithNamedFlowRunVectorBuilder());
        }

        public void Traverse(IFlowConfigurer root, IFlowConfigurer leaf, Guid correlationIdFollowingLast)
        {
            var current = leaf;
            var precedingCorrelationId = correlationIdFollowingLast;
            
            while (current != null)
            {
                var runVector = BuildRunVector(current, precedingCorrelationId);

                AddNewRunVector(runVector);

                if(current == root)
                    break;

                precedingCorrelationId = current.CorrelationId;
                current = current.Previous;
            }
        }

        public Guid FindJoinCorrelationIdForJoinpointName(string joinpointName)
        {
            Guid result;

            if (!_joinpointNameToCorrelationId.TryGetValue(joinpointName, out result))
            {
                var config = GetFlowConfigurationForFlowName(_flowName, _flowConfigurations);
                var joinpointConfigurer = config.LastFlowConfigurer.FindAll(x => x is IJoinpointFlowConfigurer)
                      .Cast<IJoinpointFlowConfigurer>()
                      .FirstOrDefault(x => x.JoinpointName == joinpointName);

                if(joinpointConfigurer == null)
                    throw new JoinpointNotFoundException(joinpointName);

                _joinpointNameToCorrelationId.Add(joinpointName, joinpointConfigurer.CorrelationId);

                result = joinpointConfigurer.CorrelationId;
            }

            return result;
        }

        public IRunVector BuildRunVector(IFlowConfigurer flowConfigurer, Guid nextCorrelationId)
        {
            var builder = GetRunVectorBuilderForFlowConfigurer(flowConfigurer);

            if(builder == null)
                throw new InvalidOperationException("No run vector builder found for flow configurer!");

            var result = builder.BuildRunVector(flowConfigurer, nextCorrelationId, this);

            return result;
        }

        public void SetCorrelationIdForJoinpointName(string joinpointName, Guid correlationId)
        {
            if(_joinpointNameToCorrelationId.ContainsKey(joinpointName) && _joinpointNameToCorrelationId[joinpointName] != correlationId)
                throw new DuplicateJoinpointException(joinpointName);
            if(!_joinpointNameToCorrelationId.ContainsKey(joinpointName))
                _joinpointNameToCorrelationId.Add(joinpointName, correlationId);
        }

        private IRunVectorBuilder GetRunVectorBuilderForFlowConfigurer(IFlowConfigurer flowConfigurer)
        {
            var current = _runVectorBuilders.First;

            while (current != null && !current.Value.CanBuildRunVectorFromFlowConfigurer(flowConfigurer))
            {
                current = current.Next;
            }

            return current.Value;
        }

        private void AddNewRunVector(IRunVector runVector)
        {
            _runVectors.Add(runVector);
        }

        public IFlowStack BuildUpFlowStack()
        {
            _runVectors.Clear();
            _joinpointNameToCorrelationId.Clear();

            var configuration = GetFlowConfigurationForFlowName(_flowName, _flowConfigurations);

            if(configuration == null) throw new FlowNotFoundException(_flowName);

            var root = configuration.LastFlowConfigurer.GetRoot();

            Traverse(null, configuration.LastFlowConfigurer, Guid.Empty);

            var result = new SimpleFlowStack(_flowName, root.CorrelationId, _runVectors);

            _whenBuilt(result);

            return result;
        }

        private static FlowConfiguration GetFlowConfigurationForFlowName(string flowName, IEnumerable<FlowConfiguration> flowConfigurations)
        {
            return flowConfigurations.FirstOrDefault(x => x.Name == flowName);
        }

        #region IDisposable implementation

        public void Dispose()
        {
            _runVectorBuilders.Clear();
            _runVectors.Clear();
            _joinpointNameToCorrelationId.Clear();
            _whenBuilt = null;
        }

        #endregion
    }
}