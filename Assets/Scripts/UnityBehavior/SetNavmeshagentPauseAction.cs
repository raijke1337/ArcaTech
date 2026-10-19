using System;
using Arcatech.Units;
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
    [SerializeReference] public BlackboardVariable<float> Timeout = new (1);

    private float timePassed;
    private NavMeshAgent _agent;
    private NPCBehaviorWrapper _wrapper;
    protected override Status OnStart()
    {
        timePassed = 0;
        _agent = Agent.Value.GetComponent<NavMeshAgent>();

        // у врагов пауза идёт через wrapper: машина состояний (CanMove) её не перезапишет,
        // а если агент сейчас не на навмеше, wrapper применит её, когда вернётся
        if (_wrapper == null) Agent.Value.TryGetComponent(out _wrapper);
        if (_wrapper != null)
        {
            _wrapper.BehaviorHold = Value.Value;
            return Status.Success;
        }
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_agent.isOnNavMesh)
        {
            _agent.isStopped = Value.Value;
        return Status.Success;
            
        }
        timePassed += Time.deltaTime;
        if (timePassed >= Timeout.Value) return Status.Failure;
        
        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

