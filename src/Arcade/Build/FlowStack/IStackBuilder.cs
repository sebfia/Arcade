using System;
using Arcade.Dsl;
using Arcade.Run.RunVectors;

namespace Arcade.Build.FlowStack
{
    public interface IStackBuilder
    {
        void Traverse(IFlowConfigurer root, IFlowConfigurer leaf, Guid correlationIdOfRunVectorFollowingLast);
        IRunVector BuildRunVector(IFlowConfigurer flowConfigurer, Guid nextCorrelationId);
        Guid FindJoinCorrelationIdForJoinpointName(string joinpointName);
        void SetCorrelationIdForJoinpointName(string joinpointName, Guid correlationId);
    }
    
}