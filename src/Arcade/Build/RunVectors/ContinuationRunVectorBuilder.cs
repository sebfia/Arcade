using System;
using System.Reflection;
using Arcade.Build.FlowStack;
using Arcade.Dsl;
using Arcade.Dsl.Implementation;
using Arcade.Run.RunVectors;

namespace Arcade.Build.RunVectors
{
    public sealed class ContinuationRunVectorBuilder : IRunVectorBuilder
    {
        private readonly InstanceFactory _instanceFactory;
        private readonly TimeSpan _standardTimeout;

        public ContinuationRunVectorBuilder(InstanceFactory instanceFactory, TimeSpan standardTimeout)
        {
            _instanceFactory = instanceFactory;
            _standardTimeout = standardTimeout;
        }

        public bool CanBuildRunVectorFromFlowConfigurer(IFlowConfigurer flowConfigurer)
        {
            return flowConfigurer is IContinuationFlowConfigurer;
        }

        public IRunVector BuildRunVector(IFlowConfigurer flowConfigurer, Guid nextCorrelationId, IStackBuilder stackBuilder)
        {
            var configurer = flowConfigurer as IContinuationFlowConfigurer;
            var correlationId = configurer.CorrelationId;
            var timeout = GetTimeout(configurer.ContinuationType);
            var instance = CreateInstance(configurer);
            var boxingContinuation = configurer.CreateBoxingContinuation();
            var targetMethodInfo = boxingContinuation.GetType().GetMethod("Process");
            var inputMethod = FindInputMethod(configurer);
            var outputSignature = GetOutputSignature(inputMethod);
            var description = instance.ToString();

            return new ContinuationRunVector(correlationId, nextCorrelationId, instance, timeout, inputMethod, outputSignature, boxingContinuation, targetMethodInfo, description);
        }

        private static Type GetOutputSignature(MethodInfo implementedInterfaceMethod)
        {
            return implementedInterfaceMethod.GetParameters()[1].ParameterType;
        }

        private static MethodInfo FindInputMethod(IContinuationFlowConfigurer flowConfigurer)
        {
            return flowConfigurer.ImplementedInterfaceType.GetMethods()[0];
        }

        private object CreateInstance(IContinuationFlowConfigurer flowConfigurer)
        {
            var selfCreating = flowConfigurer as ISelfCreatingFlowConfigurer;

            if (selfCreating != null)
            {
                return selfCreating.CreateInstance();
            }

            return _instanceFactory.CreateInstance(flowConfigurer.ContinuationType, flowConfigurer.CorrelationId);
        }

        private TimeSpan GetTimeout(Type continuationType)
        {
            var customTimeout = Attribute.GetCustomAttribute(continuationType, typeof(CustomTimeoutAttribute)) as CustomTimeoutAttribute;

            if (customTimeout != null)
            {
                return customTimeout.TimeoutInSeconds.Seconds();
            }

            return _standardTimeout;
        }
    }
    
}