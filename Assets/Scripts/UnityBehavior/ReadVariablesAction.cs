using System;
using Arcatech.Units;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ReadVariables (Spider)", story: "[Agent] finds [Wrapper]", category: "Action/Game/Spider", id: "988f27fc0e62e37947b1ab6d2475505b")]
public partial class ReadVariablesAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<NPCBehaviorWrapper> Wrapper;
    [SerializeReference] public BlackboardVariable<float> MeleeRange;
    [SerializeReference] public BlackboardVariable<float> RangedRange;


    protected override Status OnStart()
    {
        Wrapper.Value = Agent.Value.GetComponent<NPCBehaviorWrapper>();
        return Wrapper.Value == null ? Status.Failure : Status.Running;
    }

    protected override Status OnUpdate()
    {
        MeleeRange.Value = Wrapper.Value.Config.MeleeRange;
        RangedRange.Value = Wrapper.Value.Config.RangedRange;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

