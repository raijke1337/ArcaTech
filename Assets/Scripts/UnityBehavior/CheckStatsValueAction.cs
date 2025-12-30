using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Check Stats Value", story: "Agent checks STAT % to be COMPARISON than [VALUE]", category: "Action/Conditional", id: "1f2b6aa44e9147f6fbf59481908b4c68")]
public partial class CheckStatsValueAction : Action
{
    [SerializeReference] public BlackboardVariable<float> VALUE;

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

