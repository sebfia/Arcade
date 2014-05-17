using System;
using Arcade.Run.Execution;

namespace Arcade.Run.RunVectors
{
	public static class Dev
	{
		public static void Null(Result result)
		{
			System.Diagnostics.Debug.WriteLine ("The following result went to /dev/null: " + result);
		}
	}

    public abstract class BoxingContinuationBase
    {
        protected Action<Result> Continuation;
        
        protected BoxingContinuationBase()
        {
            DetachContinuation();
        }

        public void AttachContinuation(Action<Result> continueWith)
        {
            Continuation = continueWith;
        }

        public void DetachContinuation()
        {
			Continuation = Dev.Null;
        }
    }

    public sealed class BoxingContinuation<T> : BoxingContinuationBase
    {
        public void Process(T input)
        {
            Continuation(Result.FromValue(input));
        }
    }


    public sealed class BoxingContinuation : BoxingContinuationBase
    {
        public void Process()
        {
            Continuation(Result.Empty);
        }
    }
}