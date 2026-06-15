using System;
using Arcatech.Units;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Effect is active", story: "[Wrapper] checks if Effect with [EffectID] is active", category: "Conditions/Game", id: "2e48b361edd1c14042237c20bd356127")]
public partial class UnitHasEffectActive : Condition
{
    [SerializeReference] public BlackboardVariable<NPCBehaviorWrapper> Wrapper;
    [SerializeReference] public BlackboardVariable<string> EffectID;

    public override bool IsTrue()
    {
        return Wrapper.Value.HasEffect(EffectID.Value);
    }
}