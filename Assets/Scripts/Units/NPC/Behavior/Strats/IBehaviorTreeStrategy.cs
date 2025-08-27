using Arcatech.AI;
using Arcatech.Stats;

namespace Arcatech.Units.Behaviour
{
    public interface IBehaviorTreeStrategy : IStrategy
    {
        Node.NodeStatus Process(NPCUnitComponent actor);
        void Reset();
    }


    //public class ITacticsRequestAction : IBehaviorTreeStrategy
    //{
    //    RoomUnitsGroup _g;
    //    ITacticsRequest _r;
    //    public ITacticsRequestAction(RoomUnitsGroup group, ITacticsRequest r)
    //    {
    //        _g = group;
    //        _r = r;
    //    }

    //    public Node.NodeStatus Process(ControlledUnit actor)
    //    {
    //        if (_g.ProcessTacticsRequest(_r) == null) return Node.NodeStatus.Fail;

    //    }

    //    public void Reset()
    //    {

    //    }
    //}
}