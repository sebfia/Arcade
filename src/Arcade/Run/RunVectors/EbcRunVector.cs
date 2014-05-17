using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Arcade.Run.Execution;

namespace Arcade.Run.RunVectors
{
    /// <summary>
    /// A run vector that wraps an event based component.
    /// </summary>
    public sealed class EbcRunVector : IExecutableRunVector
    {
        private readonly object _instance;
        private readonly MethodInfo _method;
        private readonly EventInfo _output;
        private readonly MethodInfo _outputTargetMethod;
        private readonly string _description;
        private readonly BoxingContinuationBase _outputTarget;
        private readonly Guid _correlationId;
        private readonly Guid _nextCorrelationId;
        private readonly TimeSpan _timeout;

        public EbcRunVector(Guid correlationId, Guid nextCorrelationId, object flowInstance, TimeSpan timeout, MethodInfo inputMethodInfo, EventInfo outputEventInfo, BoxingContinuationBase boxingContinuation, MethodInfo targetBoxingMethodInfo, string description)
        {
            _correlationId = correlationId;
            _nextCorrelationId = nextCorrelationId;
            _instance = flowInstance;
            _timeout = timeout;
            _method = inputMethodInfo;
            _output = outputEventInfo;
            _outputTarget = boxingContinuation;
            _outputTargetMethod = targetBoxingMethodInfo;
            _description = description;
        }

        public Guid CorrelationId
        {
            get { return _correlationId; }
        }

        public Guid NextCorrelationId
        {
            get
            {
                return _nextCorrelationId;
            }
        }

        public TimeSpan Timeout
        {
            get { return _timeout; }
        }

        private void Attach(Action<Result> continuation)
        {
            _outputTarget.AttachContinuation(continuation);
            var invoker = CreateInvoker();
            _output.AddEventHandler(_instance, invoker);
        }

        private void Detach()
        {
            _outputTarget.DetachContinuation();
            var invoker = CreateInvoker();
            _output.RemoveEventHandler(_instance, invoker);
        }

        public void Run(ExecutePackage executePackage, Action<Tuple<Result, Exception>> whenFinished)
        {
            var input = (_method.GetParameters().Length == 0) ? null : executePackage.ExecuteMessage.Parameter.Value;
            var timeout = executePackage.RunVector.Timeout;

            var ct = executePackage.CancellationToken;

            var synchronizer = new ManualResetEventSlim(false);
            var registration = ct.Register(synchronizer.Set);

            Result result = null;
            Exception exception = null;

            Attach (r =>
            {
                result = r;

                if (!synchronizer.IsSet)
                {
                    synchronizer.Set();
                }
                else
                {
                    synchronizer.Dispose();
                }
            });

            var invoker = CreateInvoker();

            var state = Tuple.Create (_method, _instance, input, invoker, new Action<Exception> (ex => {
                exception = ex;

                if (!synchronizer.IsSet)
                {
                    synchronizer.Set();
                }
                else
                {
                    synchronizer.Dispose();
                }
            }));

            ThreadPool.QueueUserWorkItem(QueueMethodInvocation, state);

            if(!synchronizer.Wait(timeout))
            {
                whenFinished (new Tuple<Result, Exception>(null, new TimeoutException ()));
            }
            else
            {
                if (result != null) // successful finish
                    whenFinished(new Tuple<Result, Exception>(result, exception));
                else if (exception != null) // an exception has occurred
                    whenFinished(new Tuple<Result, Exception>(Result.Null, exception));
                else // a cancellation has happened
                    whenFinished(new Tuple<Result, Exception>(null, new TaskCanceledException()));
            }

            registration.Dispose();
            synchronizer.Dispose();

            Detach ();
        }

        private static void QueueMethodInvocation(object state)
        {
            var tools = state as Tuple<MethodInfo,object, object, Delegate, Action<Exception>>;

            var mi = tools.Item1;
            var instance = tools.Item2;
            var input = tools.Item3;
            var continuation = tools.Item4;
            var onException = tools.Item5;

            var parameters = (input != null) ? (new [] { input }) : (new object[0]);

            try
            {
                mi.Invoke (instance, BindingFlags.InvokeMethod, null, parameters, CultureInfo.InvariantCulture);
            }
            catch(Exception ex)
            {
                if (onException != null)
                    onException (ex);
            }
        }

        private Delegate CreateInvoker()
        {
            return Delegate.CreateDelegate(_output.EventHandlerType, _outputTarget, _outputTargetMethod);
        }

        public override string ToString()
        {
            return _description;
        }
    }
}