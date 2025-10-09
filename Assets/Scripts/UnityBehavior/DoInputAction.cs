using System;
using Arcatech;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Do input action", story: "[Agent] uses [inputAction]", category: "Action/Game", id: "ad61333688a7a94462659bcbe4546dfd")]
public partial class DoInputAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<UnitInputAction> InputAction;

    private ActiveGameUnitComponent comp;

    private bool bandaid = false;
    protected override Status OnStart()
    {
        bandaid = false;
        return !Agent.Value.TryGetComponent(out comp) ? Status.Failure : Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (bandaid) return Status.Success;
        bandaid = true;
        switch (InputAction.Value)
        {

            case UnitInputAction.MeleeAttack:
                return comp.Command(UnitActionType.Melee) ? Status.Success : Status.Failure;
            case UnitInputAction.RangedAttack:
                return comp.Command(UnitActionType.Ranged) ? Status.Success : Status.Failure;
            case UnitInputAction.MeleeSkill:
                return comp.Command(UnitActionType.MeleeSkill) ? Status.Success : Status.Failure;
            case UnitInputAction.RangedSkill:
                return comp.Command(UnitActionType.RangedSkill) ? Status.Success : Status.Failure;
            case UnitInputAction.DodgeSkill:
                return comp.Command(UnitActionType.DodgeSkill) ? Status.Success : Status.Failure;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
    }

    protected override void OnEnd()
    {
        bandaid = false;
    }
}

