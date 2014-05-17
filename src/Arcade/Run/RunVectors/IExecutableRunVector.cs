using System;
using Arcade.Run.Execution;

namespace Arcade.Run.RunVectors
{
    /// <summary>
    /// If implemented by a <see cref="IRunVector"/> is able to run on itself within a
    /// <see cref="IRunFlowStack"/>
    /// </summary>
    public interface IExecutableRunVector : IRunVector
    {
        void Run (ExecutePackage executePackage, Action<Tuple<Result, Exception>> whenFinished);
    }
}