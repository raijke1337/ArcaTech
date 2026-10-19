using System;
using Arcatech.Units.Combat;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

/// <summary>
/// Возвращает токен атаки координатору. Всегда Success, даже если токена не было,
/// поэтому её можно ставить и в ветку-уборку.
/// ResetAggression: true после настоящей атаки (агрессия падает до AggressionAfterAttack),
/// false — если атака не состоялась.
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Release Attack Token",
    story: "[Agent] releases attack token",
    category: "Action/Combat",
    id: "b2fdb4efad1acd5d184e08204b214395")]
public partial class ReleaseAttackTokenAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<bool> ResetAggression = new(true);

    protected override Status OnStart()
    {
        var go = Agent?.Value;
        if (go != null && go.TryGetComponent(out CombatParticipant participant) && participant.Coordinator != null)
            participant.Coordinator.ReleaseToken(participant, ResetAggression.Value);

        return Status.Success;
    }
}
