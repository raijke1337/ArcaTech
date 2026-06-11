using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Rotate towards", story: "[Agent] rotates to face [Target] at [Speed] degrees per second, with view cone of [Angle]", category: "Action/Navigation", id: "0c7894569c4abed86e570e722cb32237")]
public partial class RotateTowardsAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Angle;
    [SerializeReference] public BlackboardVariable<float> Speed;
    // is rotation clockwise?

    protected override Status OnStart()
    {
        if (Agent.Value == null || Target.Value == null) return Status.Failure;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        
        // Calculate direction to target
        Vector3 directionToTarget = (Target.Value.transform.position - Agent.Value.transform.position).normalized;
        
        directionToTarget.y = 0f;
        directionToTarget = directionToTarget.normalized;
        
        // Calculate current forward direction
        Vector3 currentForward = new Vector3(Agent.Value.transform.forward.x, 0f, Agent.Value.transform.forward.z).normalized;
        
        // Calculate angle between current forward and target direction
        float currentAngle = Vector3.Angle(currentForward, directionToTarget);
        
        // Check if within tolerance
        if (currentAngle <= Angle.Value)
        {
            return Status.Success;
        }
        

        
        // Calculate target rotation
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        
        // Smoothly rotate toward target
        float rotationStep = Speed.Value * Time.deltaTime;
        
        
        Agent.Value.transform.rotation = Quaternion.RotateTowards(Agent.Value.transform.rotation, targetRotation, rotationStep);
        
        return Status.Running;
        
    }
}

