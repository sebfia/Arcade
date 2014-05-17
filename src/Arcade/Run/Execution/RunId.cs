using System;

namespace Arcade.Run.Execution
{
    public sealed class RunId : IEquatable<RunId>
    {
        private readonly RunId _parent;
        private readonly string _flowName;
        private readonly Guid _id;

        private RunId(RunId parent, string flowName, Guid id)
        {
            _parent = parent;
            _flowName = flowName;
            _id = id;
        }

        public static RunId New(string flowName)
        {
            return new RunId(null, flowName, Guid.NewGuid());
        }

        public RunId NewChild(string flowName)
        {
            return new RunId(this, flowName, Guid.NewGuid());
        }

        public bool HasParent
        {
            get { return _parent != null; }
        }

        public RunId GetParent()
        {
            if(!HasParent) throw new InvalidOperationException("This run id has no parent! Check HasParent first to avoid this exception.");

            return _parent;
        }

        public RunId GetRoot()
        {
            if (!HasParent) return this;

            var current = this;

            while (current.HasParent)
            {
                current = current.GetParent();
            }

            return current;
        }

        public bool Equals(RunId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(_parent, other._parent) && string.Equals(_flowName, other._flowName) && _id.Equals(other._id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is RunId && Equals((RunId) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (_parent != null ? _parent.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (_flowName != null ? _flowName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ _id.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(RunId left, RunId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RunId left, RunId right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return _flowName;
        }

        public static implicit operator string(RunId value)
        {
            return value.ToString();
        }
    }
}