using Arcatech.Units;
using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "GetRequiredComponents", story: "[Agent] performs GetComponent for [Wrapper], finds [Allies]", category: "Action/Game", id: "f4ea2558cada2b00920b05bcd4caee94")]
public partial class GetRequiredComponentsAction : Action
{
    
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<NPCBehaviorWrapper> Wrapper;
    [SerializeReference] public BlackboardVariable<List<GameObject>> Allies;

    protected override Status OnStart()
    {
        if (!Agent.Value.TryGetComponent<NPCBehaviorWrapper>(out var w))
        {
            return  Status.Failure;
        }
        Wrapper.Value = w;
        return Status.Success;
    }

}

