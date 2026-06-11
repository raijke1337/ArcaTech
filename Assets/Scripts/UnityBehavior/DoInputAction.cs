using System;
using Arcatech.Units;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Queue input action", story: "[Wrapper] uses [inputAction]", category: "Action/Game", id: "ad61333688a7a94462659bcbe4546dfd")]
public partial class QueueAction : Action
{
    [SerializeReference] public BlackboardVariable<NPCBehaviorWrapper> Wrapper;
    [SerializeReference] public BlackboardVariable<UnitActionType> InputAction;

    protected override Status OnStart()
    {
        if (Wrapper.Value == null || !Wrapper.Value.ActionAvailable(InputAction.Value)) return Status.Failure;
        
        return Wrapper.Value.RequestAction(InputAction.Value) ?  Status.Success : Status.Failure;
    }

}

