using Arcade.Run.Messages;

namespace Arcade.Run.Observers
{
    public interface IRuntimeMessageObserver<in TRuntimeMessage> where TRuntimeMessage : IRuntimeMessage
    {
        void Observe(TRuntimeMessage runtimeMessage);
    }
    
}