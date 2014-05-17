using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Arcade.Run.Execution;

namespace Arcade.Run.RunVectors
{
    public sealed class ContinuationRunVector : IExecutableRunVector
    {
        private readonly Guid _correlationId;
        private readonly TimeSpan _timeout;
        private readonly Guid _nextCorrelationId;
        private readonly BoxingContinuationBase _outputTarget;
        private readonly MethodInfo _outputTargetMethod;
        private readonly string _description;
        private readonly object _instance;
        private readonly MethodInfo _method;
        private readonly Type _output;

        public ContinuationRunVector(Guid correlationId, Guid nextCorrelationId, object continuationInstance, TimeSpan timeout, MethodInfo inputMethodInfo, Type outputSignature, BoxingContinuationBase boxingContinuation, MethodInfo targetBoxingMethodInfo, string description)
        {
            _correlationId = correlationId;
            _nextCorrelationId = nextCorrelationId;
            _instance = continuationInstance;
            _timeout = timeout;
            _method = inputMethodInfo;
            _output = outputSignature;
            _outputTarget = boxingContinuation;
            _outputTargetMethod = targetBoxingMethodInfo;
            _description = description;
        }

        public Guid CorrelationId
        {
            get { return _correlationId; }
        }

        public TimeSpan Timeout
        {
            get { return _timeout; }
        }

        public Guid NextCorrelationId
        {
            get { return _nextCorrelationId; }
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

            _outputTarget.AttachContinuation(r =>
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

            var state = Tuple.Create(_method, _instance, input, invoker, new Action<Exception>(ex =>
            {
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

            if(!synchronizer.Wait(timeout)) // a timeout
            {
                whenFinished (new Tuple<Result, Exception>(Result.Null, new TimeoutException()));
            }
            else
            {
                if (result != null) // successful finish
                    whenFinished(new Tuple<Result, Exception>(result, exception));
                else if(exception != null) // an exception has occurred
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

            var parameters = (input != null) ? (new [] { input, continuation }) : (new [] { continuation });

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

        private void Detach()
        {
            _outputTarget.DetachContinuation();
        }

        private Delegate CreateInvoker()
        {
            return Delegate.CreateDelegate(_output, _outputTarget, _outputTargetMethod);
        }

        public override string ToString()
        {
            return _description;
        }
    }
}