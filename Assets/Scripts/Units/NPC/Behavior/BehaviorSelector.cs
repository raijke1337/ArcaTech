namespace Arcatech.Units.Behaviour
{
    /// <summary>
    /// selector is the OR of behavior trees
    /// if any of the children succeed returns success
    /// </summary>
    public class BehaviorSelector : Node
    {

        public BehaviorSelector(string n, int p = 0) : base(n, p) { }

        public override NodeStatus Process(NPCUnitComponent actor)
        {
            if (currentChildIndex < Children.Count)
            {
                switch (Children[currentChildIndex].Process(actor))
                {
                    case NodeStatus.Running:
                        return NodeStatus.Running;

                    case NodeStatus.Success:
                        Reset();
                        return NodeStatus.Success;
                    default:
                        currentChildIndex++;
                        return NodeStatus.Running;
                }
            }
            Reset();
            return NodeStatus.Fail;
        }
    }

}