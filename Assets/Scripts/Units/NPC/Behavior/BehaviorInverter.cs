namespace Arcatech.Units.Behaviour
{
    /// <summary>
    /// retruns the opposite of the result of child
    /// </summary>
    public class BehaviorInverter : Node  
    {
        public BehaviorInverter (string n) : base (n)
        {

        }

        public override NodeStatus Process(NPCUnitComponent actor)
        {
            switch (Children[currentChildIndex].Process(actor))
            {
                case NodeStatus.Success:
                    return NodeStatus.Fail;

                case NodeStatus.Fail:
                    return NodeStatus.Success;

                default:
                    return NodeStatus.Running;
            }

        }


    }


}