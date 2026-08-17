using Arcatech.Units;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Read config drone", story: "[Agent] reads configs from [Wrapper]", category: "Action/Game/GemDron", id: "2bfc561f1bc3c4d578948c4093bf4806")]
public partial class ReadConfigDroneAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<NPCBehaviorWrapper> Wrapper;

    [SerializeReference] public BlackboardVariable<float> RunAwayDisatance;
    [SerializeReference] public BlackboardVariable<float> RangedAttackDistance;
    protected override Status OnStart()
    {
        return Status.Running;
    }


    protected override Status OnUpdate()
    {
        var cfg = Wrapper.Value.Config;
        RunAwayDisatance.Value = cfg.MeleeRange;
        RangedAttackDistance.Value = cfg.RangedRange;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

