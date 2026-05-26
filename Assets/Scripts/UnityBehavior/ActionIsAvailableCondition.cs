using System;
using Arcatech.Units;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Action is available", story: "[Wrapper] checks if [action] is possible", category: "Conditions/Game", id: "2f48b361edd1c14042237c20bd356127")]
public partial class ActionIsAvailableCondition : Condition
{
    [SerializeReference] public BlackboardVariable<NPCBehaviorWrapper> Wrapper;
    [SerializeReference] public BlackboardVariable<UnitActionType> Action;

    public override bool IsTrue()
    {
        return Wrapper.Value.ActionAvailable(Action.Value);
    }

}
