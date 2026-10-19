using System;
using Arcatech.Units;
using Arcatech.Units.Combat;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

/// <summary>
/// Держит врага в слоте, который назначил координатор цели: держатель токена ближнего боя
/// идёт во внутреннее кольцо, остальные ждут во внешнем. Работает постоянно (Running),
/// ставится в отдельную ветку Parallel вместо Chase Transform / Navigate To Target.
/// Паузу навигации не трогает (кроме старта): её ставят ветки атак.
/// Пока враг атакует (Request вернул Success, Release ещё не вызван), он стоит на месте
/// и поворачивается к цели: машина состояний снимает паузу агента между ударами,
/// и без этого враг уходил бы к слоту посреди серии.
/// Без координатора или без слота ведёт себя как преследование цели.
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Move To Engagement Slot",
    story: "[Agent] holds engagement slot around [Target]",
    category: "Action/Combat",
    id: "1d2e5ee5dcbfa626378237c438eb52c8")]
public partial class MoveToEngagementSlotAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<float> StoppingDistance = new(0.3f);

    /// <summary>Поворачиваться к цели, стоя в слоте.</summary>
    [SerializeReference] public BlackboardVariable<bool> FaceTarget = new(true);

    /// <summary>&gt; 0 — своя скорость (например $CombatMS Боилера), иначе скорость из wrapper.</summary>
    [SerializeReference] public BlackboardVariable<float> SpeedOverride = new(0f);

    private const float RepathThreshold = 0.35f;

    private NPCBehaviorWrapper _wrapper;
    private NavMeshAgent _nav;
    private CombatParticipant _participant;
    private CombatCoordinator _coordinator;
    private Vector3 _lastDestination;
    private bool _hasDestination;

    protected override Status OnStart()
    {
        var go = Agent?.Value;
        if (go == null || Target?.Value == null)
        {
            LogFailure("[MoveToEngagementSlot] Agent or Target is null");
            return Status.Failure;
        }

        if (_wrapper == null) go.TryGetComponent(out _wrapper);
        if (_nav == null && !go.TryGetComponent(out _nav))
        {
            LogFailure("[MoveToEngagementSlot] NavMeshAgent not found");
            return Status.Failure;
        }
        if (_participant == null) _participant = CombatParticipant.Of(go);
        if (!_nav.isOnNavMesh) return Status.Failure;

        _coordinator = CombatCoordinator.FindFor(Target.Value);
        if (_coordinator != null) _participant.Engage(_coordinator);

        // как Chase Transform: снимаем паузу на старте, но не посреди атаки
        if (!_participant.HasAnyToken)
        {
            if (_wrapper != null) _wrapper.BehaviorHold = false;
            else _nav.isStopped = false;
        }

        _hasDestination = false;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_nav == null || !_nav.enabled || !_nav.isOnNavMesh) return Status.Failure;
        var target = Target.Value;
        if (target == null) return Status.Failure;
        if (_wrapper != null && _wrapper.ImpulseActive) return Status.Running; // агентом рулит ImpulseApplier

        _participant.NotifySlotRequested();

        if (_participant.IsCommitted)
        {
            if (_nav.hasPath) _nav.ResetPath();
            _hasDestination = false;
            return Status.Running;   // без Face: целится только Rotate towards в графе
        }

        _nav.speed = SpeedOverride.Value > 0f
            ? SpeedOverride.Value
            : _wrapper != null ? _wrapper.CurrentMoveSpeed : _nav.speed;

        Vector3 destination;
        if (_coordinator != null && _participant.HasSlot)
        {
            destination = _participant.SlotPosition;
            _nav.stoppingDistance = StoppingDistance.Value;
        }
        else
        {
            // запасной вариант: преследуем цель и останавливаемся на дистанции удара
            destination = target.position;
            _nav.stoppingDistance = _participant.AttackRange * 0.8f;
        }

        if (!_hasDestination || (destination - _lastDestination).sqrMagnitude > RepathThreshold * RepathThreshold)
        {
            _nav.SetDestination(destination);
            _lastDestination = destination;
            _hasDestination = true;
        }

        if (FaceTarget.Value) FaceWhenIdle(target);
        return Status.Running;
    }

    private void FaceWhenIdle(Transform target)
    {
        if (_nav.isStopped || (_wrapper != null && _wrapper.BehaviorHold) || !_nav.updateRotation || _nav.pathPending) return;
        if (_nav.velocity.sqrMagnitude > 0.04f) return;

        Face(target);
    }

    private void Face(Transform target)
    {
        if (!_nav.updateRotation) return; // root motion: поворотом управляет анимация

        Vector3 to = target.position - _nav.transform.position;
        to.y = 0f;
        if (to.sqrMagnitude < 0.01f) return;

        float speed = _wrapper != null && _wrapper.Config != null ? _wrapper.Config.RotateSpeed : 120f;
        var t = _nav.transform;
        t.rotation = Quaternion.RotateTowards(t.rotation, Quaternion.LookRotation(to), speed * Time.deltaTime);
    }
}
