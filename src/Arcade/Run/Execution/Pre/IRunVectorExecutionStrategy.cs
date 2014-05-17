using System;
using Arcade.Run.Execution.Messages;
using Arcade.Run.RunVectors;

namespace Arcade.Run.Execution.Pre
{
    /// <summary>
    /// The strategy to execute a <see cref="IRunVector"/> that is embedded in an <see cref="ExecutePackage"/>. 
    /// </summary>
    public interface IRunVectorExecutionStrategy
    {
        bool CanExecutePackage(ExecutePackage executePackage);
        void ExecutePackage(ExecutePackage executePackage, Action<IRunVectorExecutedMessage> continueWith);
    }
}