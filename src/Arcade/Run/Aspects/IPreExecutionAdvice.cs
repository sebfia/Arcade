using Arcade.Run.Execution;

namespace Arcade.Run.Aspects
{
    public interface IPreExecutionAdvice : IAspect
    {
        bool Handles(ExecutePackage executePackage);
        void Handle(ExecutePackage executePackage);
    }
}