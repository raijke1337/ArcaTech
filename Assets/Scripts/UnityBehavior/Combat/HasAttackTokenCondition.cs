using System;
using Arcatech.Units.Combat;
using Unity.Behavior;
using UnityEngine;

/// <summary>
/// Держит ли враг токен атаки указанной категории.
/// false, если враг ещё не зарегистрирован в координаторе (ни разу не вызывал Request / Move To Engagement Slot).
/// </summary>
[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(
    name: "Has Attack Token",
    story: "[Agent] holds [Category] attack token",
    category: "Conditions/Combat",
    id: "ac22850483fb86451771b7bb6a1160f4")]
public partial class HasAttackTokenCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<AttackTokenCategory> Category = new(AttackTokenCategory.Melee);

    private CombatParticipant _participant;

    public override bool IsTrue()
    {
        var go = Agent?.Value;
        if (go == null) return false;
        if (_participant == null && !go.TryGetComponent(out _participant)) return false;
        return _participant.HasToken(Category.Value);
    }
}
