using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Do input action", story: "Agent uses [inputAction]", category: "Action/Game", id: "ad61333688a7a94462659bcbe4546dfd")]
public partial class DoInputAction : Action
{
    [SerializeReference] public BlackboardVariable<UnitInputAction> InputAction;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

