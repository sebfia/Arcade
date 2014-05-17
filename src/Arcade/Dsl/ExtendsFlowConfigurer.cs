using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Arcade.Dsl.Implementation;

namespace Arcade.Dsl
{
    // ReSharper disable UnusedTypeParameter

    public static class ExtendsFlowConfigurer
    {
        public static IFlowConfigurer ContinueWithEbc<TFlow>(this IFlowConfigurer value)
            where TFlow : IFlow
        {
            return new EbcFlowConfigurer(value.FlowName, value, typeof(TFlow));
        }

        public static IFlowConfigurer ContinueWithEbc<TFlow>(this IFlowConfigurer value, Func<TFlow> createFlow)
            where TFlow : class, IFlow
        {
            return new SelfCreatingEbcFlowConfigurer(value.FlowName, value, typeof(TFlow), createFlow);
        }

        public static IFlowConfigurer ContinueWithNamedFlow(this IFlowConfigurer value, string flowName)
        {
            return new ContinueWithNamedFlowConfigurer(value.FlowName, value, flowName);
        }

        public static IFlowConfigurer<TOut> ContinueWithEbc<TFlow, TIn, TOut>(this IFlowConfigurer<TIn> value)
            where TFlow : IFlow<TIn, TOut>
        {
            return new EbcFlowConfigurer<TOut>(value.FlowName, value, typeof(TFlow));
        }

        public static IFlowConfigurer<TOut> ContinueWithEbc<TFlow, TIn, TOut>(this IFlowConfigurer<TIn> value, Func<TFlow> createFlow)
            where TFlow : class, IFlow<TIn, TOut>
        {
            return new SelfCreatingEbcFlowConfigurer<TOut>(value.FlowName, value, typeof(TFlow), createFlow);
        }

        public static IFlowConfigurer<TOut> ContinueWithContinuation<TContinuation, TIn, TOut>(this IFlowConfigurer<TIn> value)
            where TContinuation : IFlowContinuation<TIn, TOut>
        {
            return new ContinuationFlowConfigurer<TOut>(value.FlowName, value, typeof(TContinuation), typeof(IFlowContinuation<TIn, TOut>));
        }

        public static IFlowConfigurer<TOut> ContinueWithContinuation<TContinuation, TIn, TOut>(this IFlowConfigurer<TIn> value,
                                                                              Func<TContinuation> createContinuation)
            where TContinuation : class , IFlowContinuation<TIn, TOut>
        {
            return new SelfCreatingContinuationFlowConfigurer<TOut>(value.FlowName, value, typeof(TContinuation), typeof(IFlowContinuation<TIn, TOut>), createContinuation);
        }

        public static IFlowConfigurer<TOut> ContinueWithContinuation<TIn, TOut>(this IFlowConfigurer<TIn> value, Expression<Func<IFlowContinuation<TIn, TOut>>> create)
        {
            return new ContinuationFlowConfigurer<TOut>(value.FlowName, value, create.Body.Type, typeof(IFlowContinuation<TIn, TOut>));
        }

        public static IFlowConfigurer<TOut> ContinueWithFunction<TIn, TOut>(this IFlowConfigurer<TIn> value,
                                                                            Func<TIn, TOut> function)
        {
            return new FunctionFlowConfigurer<TOut>(value.FlowName, value, function);
        }

        public static IFlowConfigurer<TOut> ContinueWithNamedFlow<TIn, TOut>(this IFlowConfigurer<TIn> value, string flowName)
        {
            return new ContinueWithNamedFlowConfigurer<TOut>(value.FlowName, value, flowName);
        }

        public static IFlowConfigurer<TOut> ContinueWithEbc<TFlow, TOut>(this IFlowConfigurer value)
            where TFlow : IOutflow<TOut>
        {
            return new EbcFlowConfigurer<TOut>(value.FlowName, value, typeof(TFlow));
        }

        public static IFlowConfigurer<TOut> ContinueWithEbc<TFlow, TOut>(this IFlowConfigurer value, Func<TFlow> createFlow)
            where TFlow : class, IOutflow<TOut>
        {
            return new SelfCreatingEbcFlowConfigurer<TOut>(value.FlowName, value, typeof(TFlow), createFlow);
        }

        public static IFlowConfigurer ContinueWithEbc<TFlow, TIn>(this IFlowConfigurer<TIn> value)
            where TFlow : ISink<TIn>
        {
            return new EbcFlowConfigurer(value.FlowName, value, typeof(TFlow));
        }

        public static IFlowConfigurer ContinueWithEbc<TFlow, TIn>(this IFlowConfigurer<TIn> value, Func<TFlow> createFlow)
            where TFlow : class, ISink<TIn>
        {
            return new SelfCreatingEbcFlowConfigurer(value.FlowName, value, typeof(TFlow), createFlow);
        }

        public static IFlowConfigurer<TIn> BranchWhen<TIn>(this IFlowConfigurer<TIn> value, Func<TIn, bool> when, Func<IFlowConfigurer<TIn>, BranchEnd> sideBranch)
        {
            return new ConditionalFlowConfigurer<TIn>(value.FlowName, value, when, sideBranch);
        }

        public static IFlowConfigurer<TIn> GoToJoinpointIf<TIn>(this IFlowConfigurer<TIn> value, string joinpointName,
                                                                Func<TIn, bool> when)
        {
            return new GoToFlowConfigurer<TIn>(value.FlowName, value, when, joinpointName);
        }

        public static IFlowConfigurer<TOut> JoinOrContinueWithEbc<TFlow, TIn, TOut>(this IFlowConfigurer<TIn> value, string joinpointName)
            where TFlow : IFlow<TIn, TOut>
        {
            var decorated = new EbcFlowConfigurer<TOut>(value.FlowName, value, typeof(TFlow));

            return new JoinpointFlowConfigurer<TOut>(decorated, joinpointName);
        }

        public static IFlowConfigurer<TOut> JoinOrContinueWithFunction<TIn, TOut>(this IFlowConfigurer<TIn> value,
                                                                                  Func<TIn, TOut> function,
                                                                                  string joinpointName)
        {
            var decorated = new FunctionFlowConfigurer<TOut>(value.FlowName, value, function);

            return new JoinpointFlowConfigurer<TOut>(decorated, joinpointName);
        }

        public static IFlowConfigurer<TOut> JoinOrContinueWithContinuation<TContinuation, TIn, TOut>(this IFlowConfigurer<TIn> value,
                                                                          string joinpointName)
            where TContinuation : IFlowContinuation<TIn, TOut>
        {
            var decorated = new ContinuationFlowConfigurer<TOut>(value.FlowName, value, typeof (TContinuation),
                                                                 typeof (IFlowContinuation<TIn, TOut>));

            return new JoinpointFlowConfigurer<TOut>(decorated, joinpointName);
        }

        public static IFlowConfigurer<TOut> JoinAtPort<TIn, TOut>(this IFlowConfigurer<TIn> value, string joinpointName,
                                                                  string portName)
        {
            var decorated = new WaitOnPortFlowConfigurer<TOut>(value.FlowName, value, portName);

            return new JoinpointFlowConfigurer<TOut>(decorated, joinpointName);
        }

        public static IFlowConfigurer<TOut> JoinAtTrigger<T, TOut>(this IFlowConfigurer<TOut> value, Func<TOut, T> selector, string joinpointName, string triggerName)
        {
            var decorated = new TriggerFlowConfigurer<TOut>(value.FlowName, value, triggerName, selector);

            return new JoinpointFlowConfigurer<TOut>(decorated, joinpointName);
        }

        public static IFlowConfigurer<TEnumerableOut> ScatterTo<TEnumerableIn, TIn, TOut, TEnumerableOut>(this IFlowConfigurer<TEnumerableIn> value, Func<IFlowConfigurer<TIn>, Gather> scatterOperation)
            where TEnumerableIn : IEnumerable<TIn>
            where TEnumerableOut : IEnumerable<TOut>
        {
            var startScatter = new ScatterOperationFlowConfigurer<TIn>(value.FlowName, value);

            var gatherFlowConfigurer = scatterOperation(startScatter).Last;

            return new ScatterFlowConfigurer<TEnumerableOut>(value.FlowName, value, startScatter, gatherFlowConfigurer);
        }

        public static Gather Gather<TOut>(this IFlowConfigurer<TOut> value, TreatExceptionsWhenGathering treatExceptions = TreatExceptionsWhenGathering.FailFlow)
        {
            var gatherFlowConfigurer = new GatherFlowConfigurer<TOut>(value.FlowName, value, treatExceptions);
            return new Gather(gatherFlowConfigurer);
        }

        public static IFlowConfigurer<TIn> WriteState<TIn, TState>(this IFlowConfigurer<TIn> value, Func<TIn, TState> selector)
        {
            return new WriteStateFlowConfigurer<TIn>(value.FlowName, value, selector, null);
        }

        public static IFlowConfigurer<TOut> WriteState<TIn, TOut, TState>(this IFlowConfigurer<TIn> value, Func<TIn, TState> stateSelector, Func<TIn, TOut> outputSelector)
        {
            return new WriteStateFlowConfigurer<TOut>(value.FlowName, value, stateSelector, outputSelector);
        }

        public static IFlowConfigurer<TOut> ReadState<TIn, TState, TOut>(this IFlowConfigurer<TIn> value, Func<TIn, TState, TOut> combine)
        {
            return new ReadStateFlowConfigurer<TOut>(value.FlowName, value, typeof (TState), combine);
        }

        public static FlowConfiguration Exit(this IFlowConfigurer value)
        {
            return new FlowConfiguration(new FinalFlowConfigurer(value.FlowName, value));
        }

        public static BranchEnd JoinOnExit<TOut>(this IFlowConfigurer<TOut> value)
        {
            return new BranchEnd(value, String.Empty);
        }

        public static BranchEnd JoinAt<TOut>(this IFlowConfigurer<TOut> value, string joinpointName)
        {
            return new BranchEnd(value, joinpointName);
        }

        public static BranchEnd JoinOnExit(this IFlowConfigurer value)
        {
            return new BranchEnd(value);
        }

        public static BranchEnd JoinAt(this IFlowConfigurer value, string joinpointName)
        {
            return new BranchEnd(value, joinpointName);
        }

        public static IFlowConfigurer<TOut> WaitOnPort<TIn, TOut>(this IFlowConfigurer<TIn> value, string portName)
        {
            return new WaitOnPortFlowConfigurer<TOut>(value.FlowName, value, portName);
        }

        public static IFlowConfigurer<TOut> Trigger<T, TOut>(this IFlowConfigurer<TOut> value, string triggerName,
                                                                     Func<TOut, T> selector)
        {
            return new TriggerFlowConfigurer<TOut>(value.FlowName, value, triggerName, selector);
        }

        internal static IEnumerable<IFlowConfigurer> FindAll(this IFlowConfigurer value,
                                                             Func<IFlowConfigurer, bool> where, IFlowConfigurer stopAt = null)
        {
            var current = value;
            do
            {
                if(stopAt != null && current.Equals(stopAt))
                    yield break;

                if (where(current))
                    yield return current;

                if (current is IConditionalFlowConfigurer)
                {
                    var conditional = current as IConditionalFlowConfigurer;
                    var sub = conditional.JoinFlowConfigurer.FindAll(where, current);

                    foreach (var subFlowConfigurer in sub)
                    {
                        yield return subFlowConfigurer;
                    }
                }
                
                current = current.Previous;
            } while (current != null);
        }

        internal static IFlowConfigurer NextParentAfter(this IFlowConfigurer value, IFlowConfigurer root)
        {
            var current = value;

            while (current != null && current.Previous != null && !Equals(current.Previous, root))
            {
                current = current.Previous;
            }

            return current;
        }

        internal static IFlowConfigurer GetRoot(this IFlowConfigurer value)
        {
            var current = value;

            while (current.Previous != null)
            {
                current = current.Previous;
            }

            return current;
        }
    }

    // ReSharper restore UnusedTypeParameter
}