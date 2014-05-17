using System;

namespace Arcade.Dsl
{
    public class JoinpointNotFoundException : Exception
    {
        private readonly string _joinpointName;

        public JoinpointNotFoundException(string joinpointName)
        {
            _joinpointName = joinpointName;
        }

        public JoinpointNotFoundException(string joinpointName, string message)
            : base(message)
        {
            _joinpointName = joinpointName;
        }

        public string JoinpointName
        {
            get { return _joinpointName; }
        }
    }
    
}