using System;

namespace Arcade.Dsl.Implementation
{
    public sealed class GatherFlowConfigurer<T> : BaseFlowConfigurer, IGatherFlowConfigurer
    {
        private readonly TreatExceptionsWhenGathering _treatExceptions;
        private readonly Type _gatheredResultType;

        public GatherFlowConfigurer(string flowName, IFlowConfigurer previous, TreatExceptionsWhenGathering treatExceptions)
            : base(flowName, previous)
        {
            _treatExceptions = treatExceptions;
            _gatheredResultType = typeof (T);
        }

        public TreatExceptionsWhenGathering TreatExceptions
        {
            get { return _treatExceptions; }
        }

        public Type GatheredResultType
        {
            get { return _gatheredResultType; }
        }
    }
}