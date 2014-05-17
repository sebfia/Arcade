using System;
using Arcade.Dsl;

namespace Arcade.Run.RunVectors
{
    public class GatherRunVector : IRunVector
    {
        private readonly Guid _correlationId;
        
        public GatherRunVector(Guid correlationId)
        {
            _correlationId = correlationId;
        }

        public Guid CorrelationId
        {
            get { return _correlationId; }
        }

        public TimeSpan Timeout
        {
            get { return 0.Seconds(); }
        }

        public Guid NextCorrelationId
        {
            get { return Guid.Empty; }
        }

        public override string ToString()
        {
            return "Gathering";
        }
    }
}