using System;
using Arcatech.Units;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Random = UnityEngine.Random;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Pick wander point",
    story: "[Agent] picks a wander point around its StartPoint into [Target]",
    category: "Action/Navigation",
    id: "c0952e06d78e99391dd1e20ee5212386")]
public partial class PickWanderPointAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3>    Target;   // результат

    private NPCBehaviorWrapper _wrapper;

    protected override Status OnStart()
    {
        var go = Agent?.Value;
        if (go == null) { LogFailure("Agent == null"); return Status.Failure; }

        if (_wrapper == null && !go.TryGetComponent(out _wrapper))
        { LogFailure("Wrapper not found"); return Status.Failure; }

        if (_wrapper.Nav == null || !_wrapper.Nav.isOnNavMesh)
            return Status.Failure;

        float radius = _wrapper.Config.WanderRange;   // из SO
        Vector3 origin = _wrapper.StartPoint;              // рантайм-старт из Wrapper

        if (TryGetRandomNavMeshPoint(origin, radius, out var point))
        {
            Target.Value = point;
            return Status.Success;
        }

        // не нашли точку — не двигаемся, стоим на старте
        Target.Value = go.transform.position;
        return Status.Failure;
    }

    protected override Status OnUpdate() => Status.Success;

    private static bool TryGetRandomNavMeshPoint(
        Vector3 origin, float radius, out Vector3 result, int maxAttempts = 30)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 p = Random.insideUnitSphere * radius;
            p += origin;
            p.y = origin.y;

            if (NavMesh.SamplePosition(p, out var hit, radius, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = origin;
        return false;
    }
}