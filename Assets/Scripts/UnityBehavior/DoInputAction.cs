using System;
using Arcatech;
using Arcatech.Units;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Do input action", story: "[Wrapper] uses [inputAction], no checks", category: "Action/Game", id: "ad61333688a7a94462659bcbe4546dfd")]
public partial class DoInputAction : Action
{
    [SerializeReference] public BlackboardVariable<NPCBehaviorWrapper> Wrapper;
    [SerializeReference] public BlackboardVariable<UnitActionType> InputAction;

    protected override Status OnUpdate()
    {
        return Wrapper.Value.RequestAction(InputAction.Value) ? Status.Success : Status.Failure;
    }


}

