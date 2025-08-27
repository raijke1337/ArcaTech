using System.Collections.Generic;
using System.Linq;
namespace Arcatech.Units.Behaviour
{
    /// <summary>
    /// selector, but with priority
    /// </summary>
    public class BehaviourPrioritySelector : BehaviorSelector
    {
        List<Node> sortedChildren;

        public BehaviourPrioritySelector(string name, int p = 0) : base(name, p)
        {
        }

        List<Node> SortedChildren => sortedChildren ??= SortChildren();
        protected virtual List<Node> SortChildren() => Children.OrderByDescending(c => c.Priority).ToList();

        public override void Reset()
        {
            base.Reset();
            sortedChildren = null;
        }

        public override NodeStatus Process(NPCUnitComponent actor)
        {
            foreach (var c in SortedChildren)
            {
                switch (c.Process(actor))
                {
                    case NodeStatus.Running: return NodeStatus.Running;
                    case NodeStatus.Success: return NodeStatus.Success;
                    default: continue;
                }
            }
            return NodeStatus.Fail;
        }
    }


}