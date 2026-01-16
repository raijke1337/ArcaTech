using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Save current position to variable", story: "[Agent] saves current position to [variable]", category: "Action/Navigation", id: "35357863d3415eb1faed89daae80f8c2")]
public partial class SaveCurrentPositionToVariableAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> Variable;

    protected override Status OnStart()
    {
        Variable.Value = Agent.Value.transform.position; 
        return Status.Success;
    }
}

