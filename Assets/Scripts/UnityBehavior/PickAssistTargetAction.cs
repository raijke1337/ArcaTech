using Arcatech.Units;
using System;
using System.Collections.Generic;
using Arcatech.Stats;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.Rendering;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PickAssistTarget", story: "[Wrapper] Checks [List] to find ally with [stat] percent lower than [float] and assigns it into [variable]", category: "Action/Game/GemDron", id: "07415cb7e27d01e5e79c73ab14b758de")]
public partial class PickAssistTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<List<GameObject>> List;
    [SerializeReference] public BlackboardVariable<ResourceStatType> Stat;
    [SerializeReference] public BlackboardVariable<float> Float;
    [SerializeReference] public BlackboardVariable<GameObject> Variable;
    [SerializeReference] public BlackboardVariable<NPCBehaviorWrapper> Agent;
    
    protected override Status OnStart()
    {
        foreach (var ally in List.Value)
        {
            if (!ally.TryGetComponent(out NPCBehaviorWrapper stats) || stats == Agent.Value) continue;
            if (stats.GetStatPercent(Stat.Value) <= Float.Value) 
                Variable.Value = ally;
            
            return Status.Success;
        }
        return Status.Failure;
    }

}

