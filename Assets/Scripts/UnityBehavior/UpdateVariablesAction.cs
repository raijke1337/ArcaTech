using System;
using Arcatech.Stats;
using Arcatech.Units;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Update variables", story: "[Agent] updates variables: [Knockdown] status, [Stamina] value", category: "Action/Game/Player", id: "04d470dbebb82ec6cfa8d6b3efeef103")]
public partial class UpdateVariablesAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<bool> Knockdown;
    [SerializeReference] public BlackboardVariable<float> Stamina;

    private EntityStatsComponent _stats;
    private EntityStateMachineComponent _stateMachine;
    private StateMachineContext _context;
    
    protected override Status OnStart()
    {
        if (_stats == null || _stateMachine == null) return Status.Failure;
        _context ??= _stateMachine.Context;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_context == null) return Status.Failure;

        Knockdown.Value = _context.KnockDownState;
        if (!_stats.TryGetCurrent(ResourceStatType.Stamina, out var value)) return Status.Failure;
        Stamina.Value = value;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }

    protected override void OnDeserialize()
    {
        base.OnDeserialize();
        Agent.Value.TryGetComponent(out _stats);
        Agent.Value.TryGetComponent(out _stateMachine);
    }
}

