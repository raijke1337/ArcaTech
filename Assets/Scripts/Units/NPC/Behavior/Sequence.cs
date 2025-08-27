namespace Arcatech.Units.Behaviour
{
    /// <summary>
    /// sequence is similar to &&
    /// success only if all succeed
    /// </summary>
    public class Sequence : Node
    {
        public Sequence(string n,int p = 0) : base (n,p) { }

        public override NodeStatus Process(NPCUnitComponent actor)
        {
            if (currentChildIndex < Children.Count)
            {
                switch (Children[currentChildIndex].Process(actor))
                {
                    case NodeStatus.Fail:
                        Reset();
                        return NodeStatus.Fail;
                    case NodeStatus.Running:
                        return NodeStatus.Running;
                    default:
                        currentChildIndex++;
                        return currentChildIndex == Children.Count ? NodeStatus.Success : NodeStatus.Running;
                }
            }
            Reset();
            return NodeStatus.Success;
        }

    }
}