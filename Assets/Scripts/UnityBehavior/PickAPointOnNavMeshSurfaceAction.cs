using System;
using Unity.AI.Navigation;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Random = UnityEngine.Random;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Pick a point on NavMesh Surface", story: "[Agent] tries to find a point around [Start] in [range] on NavmeshSurface and sets it as [target]", category: "Action/Navigation", id: "e77d4315743160a2f4a00a4f1ffaef9a")]
public partial class PickAPointOnNavMeshSurfaceAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> Range;
    [SerializeReference] public BlackboardVariable<Vector3> Target;
    [SerializeReference] public BlackboardVariable<Vector3> Start;
    NavMeshSurface _navMeshSurface;
    NavMeshAgent _navMeshAgent;
    
    protected override Status OnStart()
    {
        _navMeshAgent = Agent.Value.GetComponent<NavMeshAgent>();
        if (!_navMeshAgent.isOnNavMesh)
        {
            return Status.Failure;
        }
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Target.Value = GetRandomNavMeshPoint(Range.Value);
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
    
    Vector3 GetRandomNavMeshPoint(float radius, int maxAttempts = 30)
    {
        Vector3 start = Start.Value;
        
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomPoint = Random.insideUnitSphere * radius;
            randomPoint += start;
            randomPoint.y = start.y;
            
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, radius, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        
        Debug.LogWarning("Could not find random NavMesh point after " + maxAttempts + " attempts");
        return Agent.Value.transform.position;
    }
}

