using System;
using Arcade.Dsl.Implementation;

namespace Arcade.Dsl
{
    public static class Flow
    {
        public static IFlowConfigurer StartWith<TFlow>(string name)
            where TFlow : IFlow
        {
            return new EbcFlowConfigurer(name, typeof(TFlow));
        }

        public static IFlowConfigurer StartWith<TFlow>(string name, Func<TFlow> createFlow)
            where TFlow : class, IFlow
        {
            return new SelfCreatingEbcFlowConfigurer(name, typeof(TFlow), createFlow);
        }

        public static IFlowConfigurer<TOut> StartWith<TFlow, TOut>(string name)
            where TFlow : IOutflow<TOut>
        {
            return new EbcFlowConfigurer<TOut>(name, typeof(TFlow));
        }

        public static IFlowConfigurer<TOut> StartWith<TFlow, TOut>(string name, Func<TFlow> createFlow)
            where TFlow : class, IOutflow<TOut>
        {
            return new SelfCreatingEbcFlowConfigurer<TOut>(name, typeof(TFlow), createFlow);
        }

        public static IFlowConfigurer<TOut> StartWith<TFlow, TIn, TOut>(string name)
            where TFlow : IFlow<TIn, TOut>
        {
            return new EbcFlowConfigurer<TOut>(name, typeof(TFlow));
        }

        public static IFlowConfigurer<TOut> StartWith<TFlow, TIn, TOut>(string name, Func<TFlow> createFlow)
            where TFlow : class, IFlow<TIn, TOut>
        {
            return new SelfCreatingEbcFlowConfigurer<TOut>(name, typeof(TFlow), createFlow);
        }

        public static IFlowConfigurer<TOut> StartWithContinuation<TContinuation, TIn, TOut>(string flowName)
            where TContinuation : IFlowContinuation<TIn, TOut>
        {
            return new ContinuationFlowConfigurer<TOut>(flowName, null, typeof(TContinuation), typeof(IFlowContinuation<TIn, TOut>));
        }

        public static IFlowConfigurer<TOut> StartWithFunc<TIn, TOut>(string flowName, Func<TIn, TOut> function)
        {
            return new FunctionFlowConfigurer<TOut>(flowName, null, function);
        }
    }
}