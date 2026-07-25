using Arcatech.Units;
using System;
using System.Collections.Generic;
using Arcatech;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Assess assist", story: "[Drone] scans [range] to find allied units of [tier] or higher, saves into [array]", category: "Action/Game/GemDron", id: "299b391b678c34cac5ed4db9d6b0da01")]
public partial class AssessAssistAction : Action
{
    [SerializeReference] public BlackboardVariable<NPCBehaviorWrapper> Drone;
    [SerializeReference] public BlackboardVariable<float> Range;
    [SerializeReference] public BlackboardVariable<UnitTier> Tier;
    [SerializeReference] public BlackboardVariable<List<GameObject>> Array;

    private Collider[] scanResults;
    
    protected override Status OnStart()
    {
        scanResults = new Collider[16];
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Physics.OverlapSphereNonAlloc(Drone.Value.transform.position, Range.Value, scanResults) > 0)
        {
            for (int i = 0; i < scanResults.Length; i++)
            {
                if (scanResults[i] != null 
                    && scanResults[i].TryGetComponent(out BaseGameEntityComponent comp) 
                    && comp.GetEntitySide == Drone.Value.Entity.GetEntitySide)
                {
                    if (comp.TryGetComponent(out ITierProvider tierProvider))
                    {
                        var tier = tierProvider.GetTierInfo;
                        if (tier >= Tier.Value)
                        {
                            Array.Value.Add(comp.gameObject);
                        }
                    }
                }
            }

            if (Array.Value.Count > 0) return Status.Success;
        }
        
        return Status.Failure;
    }

    protected override void OnEnd()
    {
    }

}

