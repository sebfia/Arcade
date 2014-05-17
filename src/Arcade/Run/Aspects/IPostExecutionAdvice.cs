using Arcade.Run.Execution.Messages;

namespace Arcade.Run.Aspects
{
    public interface IPostExecutionAdvice : IAspect
    {
        bool Handles(IRunVectorExecutedMessage executedMessage);
        void Handle(IRunVectorExecutedMessage executedMessage);
    }
}