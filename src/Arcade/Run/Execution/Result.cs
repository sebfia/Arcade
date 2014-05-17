using System;
using System.Collections;
using System.Linq;

namespace Arcade.Run.Execution
{
    public sealed class Result
    {
        public readonly object Value;
        public readonly Type Type;

        private Result()
        {
            Value = null;
            Type = typeof(void);
        }

        public Result(object value)
        {
            Value = value;
            Type = Value != null ? Value.GetType() : typeof(Nullable);
        }

        public static readonly Result Empty = new Result();
        public static readonly Result Null = new Result(null);

        public static Result FromValue<T>(T value)
        {
            return new Result(value);
        }

        public T Unbox<T>()
        {
            if(this == Empty) throw new InvalidOperationException("Unable to unbox empty result!");

            return (T)Value;
        }

        public bool IsEnumerable
        {
            get { return Value is IEnumerable; }
        }

        public Result[] EnumerateValue()
        {
            var enumerable = Value as IEnumerable;

            if (enumerable == null) throw new InvalidOperationException("Unable to enumerate non-enumerable result!");

            return enumerable.Cast<object>().Select(obj => new Result(obj)).ToArray();
        }

        public override string ToString()
        {
            return (Value != null) ? Value.ToString() : "null";
        }
    }
    
}