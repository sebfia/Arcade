using System;

namespace Arcade.Dsl
{
    public class DuplicateJoinpointException : Exception
    {
        private readonly string _joinpointName;

        public DuplicateJoinpointException(string joinpointName)
        {
            _joinpointName = joinpointName;
        }

        public DuplicateJoinpointException(string joinpointName, string message)
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