using Arcade.Run.Execution;
using Arcade.Run.Execution.Messages;

namespace Arcade.Run.RunVectors
{
    public interface ISelectableRunVector
    {
        Result Select(ExecuteRunVectorMessage message);
    }
}