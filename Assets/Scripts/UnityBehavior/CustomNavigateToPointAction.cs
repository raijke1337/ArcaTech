using System;
using Arcatech.Units;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Navigate To (live speed)",
    story: "[Agent] navigates to [Destination] using speed from Wrapper",
    category: "Action/Navigation",
    id: "a1b2c3d4e5f60718293a4b5c6d7e8f90")]
public partial class CustomNavigateToPointAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3>    Destination;
    [SerializeReference] public BlackboardVariable<float>      StoppingDistance = new(0.2f);

    private NPCBehaviorWrapper _wrapper;
    private NavMeshAgent       _nav;

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
        _nav.SetDestination(Destination.Value);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_nav == null || !_nav.isOnNavMesh) return Status.Failure;

        _nav.speed = _wrapper.CurrentMoveSpeed;         // ← подхватываем изменения КАЖДЫЙ тик

        if (_nav.pathPending) return Status.Running;

        if (_nav.remainingDistance <= _nav.stoppingDistance
            && (!_nav.hasPath || _nav.velocity.sqrMagnitude < 0.01f))
            return Status.Success;

        return Status.Running;
    }

    protected override void OnEnd()
    {
        // путь не сбрасываем здесь — это делает отдельный ResetNavigation узел,
        // чтобы поведение оставалось управляемым из графа
    }
}