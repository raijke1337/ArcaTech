using Arcatech.Stats;

namespace Arcatech.Units.Behaviour
{
    public interface IBehaviorTreeStrategy : IStrategy
    {
        Node.NodeStatus Process(ControlledUnit actor);
        void Reset();
    }
}