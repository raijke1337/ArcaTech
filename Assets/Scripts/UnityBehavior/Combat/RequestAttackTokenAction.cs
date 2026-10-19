using System;
using Arcatech.Units.Combat;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

/// <summary>
/// Просит у координатора цели токен атаки и ждёт его выдачи, затем ждёт, пока враг подойдёт на Range.
/// Success: токен у врага и он в радиусе (Range &lt;= 0 — радиус не проверяется).
/// Failure: Timeout истёк на любой из двух фаз (ожидание токена, подход), токен отозван
/// или заявку сняли (оглушение, смерть). При Failure и прерывании токен возвращается без сброса агрессии.
/// Если у цели нет CombatCoordinator — сразу Success (поведение как до координатора).
/// После атаки обязательно вызвать Release Attack Token.
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Request Attack Token",
    story: "[Agent] requests [Category] token from [Target], then approaches within [Range] m (0 = attack from where it stands)",
    category: "Action/Combat",
    id: "279f0f4fa7130e61204e1aea5f028eaa")]
public partial class RequestAttackTokenAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<AttackTokenCategory> Category = new(AttackTokenCategory.Melee);
    [SerializeReference] public BlackboardVariable<float> Range = new(0f);

    /// <summary>Сколько токенов категории занимает атака (Центурион — 2).</summary>
    [SerializeReference] public BlackboardVariable<int> Cost = new(1);

    /// <summary>Время на каждую фазу, с. Не меньше ~0.3: токены выдаются в Update координатора.</summary>
    [SerializeReference] public BlackboardVariable<float> Timeout = new(2f);

    private CombatParticipant _participant;
    private CombatCoordinator _coordinator;
    private float _deadline;
    private bool _granted;
    private bool _succeeded;

    protected override Status OnStart()
    {
        _granted = false;
        _succeeded = false;

        var go = Agent?.Value;
        var target = Target?.Value;
        if (go == null || target == null)
        {
            LogFailure("[RequestAttackToken] Agent or Target is null");
            return Status.Failure;
        }

        if (_participant == null) _participant = CombatParticipant.Of(go);
        _coordinator = CombatCoordinator.FindFor(target);
        if (_coordinator == null)
        {
            _succeeded = true;
            return Status.Success;
        }

        _participant.Engage(_coordinator);
        if (!_coordinator.HasToken(_participant, Category.Value))
            _coordinator.RequestToken(_participant, Category.Value, Cost.Value);

        _deadline = Time.time + Timeout.Value;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_participant == null || _coordinator == null) return Status.Failure;

        bool hasToken = _coordinator.HasToken(_participant, Category.Value);

        if (!_granted)
        {
            if (hasToken)
            {
                _granted = true;
                _deadline = Time.time + Timeout.Value; // фаза подхода
            }
            else
            {
                if (!_coordinator.IsRequestPending(_participant)) return Status.Failure;
                return Time.time >= _deadline ? Status.Failure : Status.Running;
            }
        }
        else if (!hasToken)
        {
            return Status.Failure; // отозван (нокдаун цели) или истёк
        }

        if (Range.Value <= 0f || InRange())
        {
            _succeeded = true;
            _coordinator.CommitToken(_participant); // атака началась: держим позицию до Release
            return Status.Success;
        }

        return Time.time >= _deadline ? Status.Failure : Status.Running;
    }

    protected override void OnEnd()
    {
        if (_succeeded || _participant == null || _coordinator == null) return;
        _coordinator.CancelRequest(_participant);
        if (_granted) _coordinator.ReleaseToken(_participant, false);
    }

    private bool InRange()
    {
        var target = Target.Value;
        return target != null &&
               Vector3.Distance(_participant.transform.position, target.position) <= Range.Value;
    }
}
