using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Change value by delta time", story: "Increment [value] by deltaTime is positive [bool]", category: "Action/Variable", id: "d42aa96d5a3541353fd3a0c938f4ebbd")]
public partial class IncrementValueByDeltaTimeAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Value;
    [SerializeReference] public BlackboardVariable<bool> Bool;
    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Bool.Value)  Value.Value += Time.deltaTime;
        else Value.Value -= Time.deltaTime;
        return Status.Running;
    }

}

