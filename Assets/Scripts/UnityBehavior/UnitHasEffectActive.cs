using System;
using Arcatech.Units;
using Arcatech.Usables.Effects;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Effect is active", story: "Check if [Wrapper] has an instance of Effect [EffectID] with state [Active]", category: "Conditions/Game", id: "2e48b361edd1c14042237c20bd356127")]
public partial class UnitHasEffectActive : Condition
{
    [SerializeReference] public BlackboardVariable<NPCBehaviorWrapper> Wrapper;
    [SerializeReference] public BlackboardVariable<string> EffectID;
    [SerializeReference] public BlackboardVariable<bool> Active;

    public override bool IsTrue()
    {
        return Wrapper.Value.HasEffect(EffectID.Value) == Active.Value;
    }
}