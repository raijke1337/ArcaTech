using System;
using Arcatech;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Check Alive Status", story: "[Agent] sets entity alive status into [variable] ", category: "Action/Game", id: "f06ee7cccc7a72f20e78b406479b160a")]
public partial class CheckAliveStatusAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<bool> Variable;

    private BaseGameEntityComponent _entityComponent;
    
    protected override Status OnStart()
    {
        if (_entityComponent == null) return Status.Failure;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Variable.Value = _entityComponent.EntityAlive;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }

    protected override void OnDeserialize()
    {
        base.OnDeserialize();
        _entityComponent =  Agent.Value.GetComponent<BaseGameEntityComponent>();
    }
}

