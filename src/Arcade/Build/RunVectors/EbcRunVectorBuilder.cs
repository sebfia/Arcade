using System;
using System.Reflection;
using Arcade.Build.FlowStack;
using Arcade.Dsl;
using Arcade.Dsl.Implementation;
using Arcade.Run.RunVectors;

namespace Arcade.Build.RunVectors
{
    public sealed class EbcRunVectorBuilder : IRunVectorBuilder
    {
        private readonly InstanceFactory _instanceFactory;
        private readonly Func<Type, MethodInfo> _findInputMethod;
        private readonly Func<Type, EventInfo> _findOutputEvent;
        private readonly TimeSpan _standardTimeout;

        public EbcRunVectorBuilder(InstanceFactory instanceFactory, 
            Func<Type, MethodInfo> findInputMethodStrategy, 
            Func<Type, EventInfo> findOutputEventStrategy,
            TimeSpan standardTimeout)
        {
            _instanceFactory = instanceFactory;
            _findInputMethod = findInputMethodStrategy;
            _findOutputEvent = findOutputEventStrategy;
            _standardTimeout = standardTimeout;
        }

        public bool CanBuildRunVectorFromFlowConfigurer(IFlowConfigurer flowConfigurer)
        {
            return (flowConfigurer is IEbcFlowConfigurer);
        }

        public IRunVector BuildRunVector(IFlowConfigurer flowConfigurer, Guid nextCorrelationId, IStackBuilder stackBuilder)
        {
            var configurer = flowConfigurer as IEbcFlowConfigurer;

            var instance = CreateInstance(configurer);
            var methodInfo = _findInputMethod(configurer.FlowType);
            var eventInfo = _findOutputEvent(configurer.FlowType);
            var box = configurer.CreateBoxingContinuation();
            var targetMethodInfo = box.GetType().GetMethod("Process");
            var timeout = GetTimeout(configurer.FlowType);
            var description = instance.ToString();

            return new EbcRunVector(flowConfigurer.CorrelationId, nextCorrelationId, instance, timeout, methodInfo, eventInfo, box, targetMethodInfo, description);
        }

        private object CreateInstance(IEbcFlowConfigurer flowConfigurer)
        {
            var selfCreating = flowConfigurer as ISelfCreatingFlowConfigurer;

            if (selfCreating != null)
            {
                return selfCreating.CreateInstance();
            }

            return _instanceFactory.CreateInstance(flowConfigurer.FlowType, flowConfigurer.CorrelationId);
        }

        private TimeSpan GetTimeout(Type flowType)
        {
            var customTimeout = Attribute.GetCustomAttribute(flowType, typeof (CustomTimeoutAttribute)) as CustomTimeoutAttribute;

            if (customTimeout != null)
            {
                return customTimeout.TimeoutInSeconds.Seconds();
            }

            return _standardTimeout;
        }
    }
    
}