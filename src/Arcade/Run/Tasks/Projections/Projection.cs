using System;
using System.Collections.Generic;
using System.Threading;
using Arcade.Run.Execution.Events;
using Arcade.Run.Messages;

namespace Arcade.Run.Tasks.Projections
{
    public static class Projection
    {
        #region ProjectionBuilder
        
        private sealed class ProjectionBuilder : IProjectionBuilder, ITaskVisitor
        {
            public static ProjectionBuilder Zero
            {
                get
                {
                    return new ProjectionBuilder();
                }
            }

            private readonly Dictionary<Type, Delegate> _handlers;
            private CancellationTokenSource _cts;
            private Guid? _contextId;

            private ProjectionBuilder()
            {
                _handlers = new Dictionary<Type, Delegate>(8);
            }

            IProjectionBuilder IProjectionBuilder.For<TEvent>(Func<TEvent, CancellationTokenSource, Guid?, IRuntimeMessage> projection)
            {
                if (_handlers.ContainsKey(typeof (TEvent)))
                    _handlers.Remove(typeof (TEvent));

                _handlers.Add(typeof(TEvent), projection);
                return this;
            }

            public void Visit(ITask task)
            {
                task.AcceptVisitor(this);
            }

            public void SetCancellationTokenSource(CancellationTokenSource cts)
            {
                _cts = cts;
            }

            public void SetContextId(Guid? contextId)
            {
                _contextId = contextId;
            }

            private IRuntimeMessage HandleEvent(IRunFlowStackEvent evt)
            {
                Delegate handler;

                if (!_handlers.TryGetValue(evt.GetType(), out handler))
                    throw new InvalidOperationException("This task does not currently handle events of this type!");

                return (IRuntimeMessage)handler.DynamicInvoke(evt, _cts, _contextId);
            }

            public Func<IRunFlowStackEvent, IRuntimeMessage> Build(ITask task)
            {
				Visit (task);
                return HandleEvent;
            }
        }

        #endregion

        public static IProjectionBuilder For<TEvent>(Func<TEvent, CancellationTokenSource, Guid?, IRuntimeMessage> projection) where TEvent : IRunFlowStackEvent
        {
            return ((IProjectionBuilder)ProjectionBuilder.Zero).For(projection);
        }

        public static IProjectionBuilder Standard
        {
            get
            {
                return 
                    (
                    For<FlowFinishedEvent>((evt, cts, ctx) => new FlowCompleteMessage(evt.RunId, evt.Result, cts, ctx))
                    .For<FlowFailedEvent>((evt, cts, ctx) => new FlowFailedMessage(evt.RunId, evt.Reason, evt.CausativeInput, evt.CorrelationId, cts, ctx))
                    .For<FlowCancelledEvent>((evt, cts, ctx) => new FlowCancelledMessage(evt.RunId, evt.CorrelationId, cts, ctx))
                    .For<TriggerEvent>((evt, cts, ctx) => new TriggerMessage(evt.RunId, evt.TriggerName, evt.Parameter))
                    .For<WaitOnPortEvent>((evt, cts, ctx) => new WaitOnPortMessage(evt.RunId, evt.CorrelationIdToContinueWith, evt.PortName, evt.Input, cts, ctx))
                    .For<InitializeChildFlowEvent>((evt, cts, ctx) => new InitializeChildFlowMessage(evt.RunId, evt.CorrelationIdToContinueWithOnParent, evt.ChildFlowName, evt.Parameter, cts, ctx))
                    .For<InitializeScatterEvent>((evt, cts, ctx) => new InitializeScatterMessage(evt.RunId, evt.OriginatingCorrelationId, evt.CorrelationIdToStartWith, evt.NextCorrelationId, evt.Parameter, evt.GatheredResultType, evt.IgnoreExceptions, cts, ctx))
                    .For<GatherFlowResultEvent>((evt, cts, ctx) => new FlowCompleteMessage(evt.RunId, evt.Result, cts, ctx))
                    );
            }
        }
    }
}