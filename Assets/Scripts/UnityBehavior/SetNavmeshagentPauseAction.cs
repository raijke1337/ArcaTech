using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set navmeshagent pause", story: "[Agent] sets NavMeshAgent Paused to [value]", category: "Action/Navigation", id: "961104aa3a55b155ddd09348acca96bd")]
public partial class SetNavmeshagentPauseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<bool> Value;

    private NavMeshAgent _agent;
    protected override Status OnStart()
    {
        _agent = Agent.Value.GetComponent<NavMeshAgent>();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        _agent.isStopped = Value.Value;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

