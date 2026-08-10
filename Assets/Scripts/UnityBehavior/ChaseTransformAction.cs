using System;
using Arcatech.Units;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Chase Transform", story: "[Agent] chases [Target]", category: "Action/Navigation", id: "20de5584d9de05287b2fce98e212b9ab")]
public partial class ChaseTransformAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<float>      StoppingDistance = new(0.2f);

    private NPCBehaviorWrapper _wrapper;
    private NavMeshAgent       _nav;
    
    private Vector3 _lastPosition;

    protected override Status OnStart()
    {
        var go = Agent?.Value;
        if (go == null) { LogFailure("Agent == null"); return Status.Failure; }

        if (_wrapper == null && !go.TryGetComponent(out _wrapper))
        { LogFailure("Wrapper not found"); return Status.Failure; }

        _nav = _wrapper.GetComponent<NavMeshAgent>();
        if (_nav == null || !_nav.isOnNavMesh) return Status.Failure;

        _nav.speed = _wrapper.CurrentMoveSpeed;         // старт с актуальной скорости
        _nav.stoppingDistance = StoppingDistance.Value;
        _nav.isStopped = false;
        _nav.SetDestination(Target.Value.position);
        _lastPosition =  Target.Value.position;
        return Status.Running;
    }
    

    protected override Status OnUpdate()
    {
        if (_nav == null || !_nav.isOnNavMesh) return Status.Failure;
        if (Target.Value.position != _lastPosition)
        {
            _nav.SetDestination(Target.Value.position);
            _lastPosition = Target.Value.position;
        }
        _nav.speed = _wrapper.CurrentMoveSpeed;       

        if (_nav.pathPending) return Status.Running;

        if (_nav.remainingDistance <= _nav.stoppingDistance
            && (!_nav.hasPath || _nav.velocity.sqrMagnitude < 0.01f))
            return Status.Success;

        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

