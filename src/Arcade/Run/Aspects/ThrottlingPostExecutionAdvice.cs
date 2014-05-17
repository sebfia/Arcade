using System;
using System.Threading;
using Arcade.Dsl;
using Arcade.Run.Execution.Messages;

namespace Arcade.Run.Aspects
{
    /// <summary>
    /// Default throttling interval = 1 second.
    /// </summary>
    public sealed class ThrottlingPostExecutionAdvice : IPostExecutionAdvice, ICanBeDeactivated
    {
        private bool _isActivated;
        private TimeSpan _throttlingInterval;

        public ThrottlingPostExecutionAdvice(TimeSpan? throttlingInterval = null)
        {
            _throttlingInterval = throttlingInterval.HasValue ? _throttlingInterval : 1.Seconds();
        }

        public void ChangeThrottleInterval(TimeSpan interval)
        {
            _throttlingInterval = interval;
        }

        public void Deactivate()
        {
            _isActivated = false;
        }

        public void Activate()
        {
            _isActivated = true;
        }

        public bool Handles(IRunVectorExecutedMessage executedMessage)
        {
            return true;
        }

        public void Handle(IRunVectorExecutedMessage executedMessage)
        {
            if (_isActivated)
                Thread.Sleep(_throttlingInterval);
        }
    }
}