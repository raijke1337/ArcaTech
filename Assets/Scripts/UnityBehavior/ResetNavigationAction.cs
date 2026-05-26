using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Reset navigation", story: "[Agent] resets NavMeshAgent destination point", category: "Action/Navigation", id: "d9df3f64654cbaaef998ca7ae1d34ab8")]
public partial class ResetNavigationAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    protected override Status OnStart()
    {
        var nav = Agent.Value.GetComponent<NavMeshAgent>();
        nav.ResetPath();
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

