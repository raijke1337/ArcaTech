using System.Collections.Generic;
using Arcatech.Triggers;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Items.Projectiles
{

    [RequireComponent(typeof(TriggerTrackerComponent), typeof(BaseGameEntityComponent))]
    public sealed class ProjectileComponent : ValidatedMonoBehaviour, IPausableComponent, ITriggerNotificationReceiver,
        ITriggerNotificationProvider
    {
        /// <summary>
        /// Ровно один раз за "жизнь" снаряда (между Reset() и следующим Reset()).
        /// </summary>
        public event UnityAction<ProjectileComponent, ProjectileFinishReason> ProjectileFinished = delegate { };

        /// <summary>
        /// Каждое столкновение, уже разобранное правилами пробития. Вызывается до реакции поведения,
        /// так что эффекты попадания применяются раньше, чем снаряд может быть уничтожен.
        /// </summary>
        public event UnityAction<ProjectileComponent, ProjectileCollision> ProjectileHit = delegate { };

        [SerializeField, Self] private BaseGameEntityComponent entity;
        public BaseGameEntityComponent Entity => entity;

        ProjectileBehavior _behavior;
        TriggerTrackerComponent _col;

        private List<IEquipmentPart> _parts;
        ITriggerNotificationReceiver _receiver;

        private BaseGameEntityComponent _owner;
        private ProjectileHitRules _rules;
        private int _hitsDone;
        private readonly HashSet<BaseGameEntityComponent> _hitTargets = new();

        private bool _finished;
        public ProjectileFinishReason LastFinishReason { get; private set; }

        private void Awake()
        {
            _col = GetComponent<TriggerTrackerComponent>();
            _parts = new List<IEquipmentPart>();
            _parts.AddRange(GetComponentsInChildren<IEquipmentPart>());
        }

        private void OnEnable()
        {
            if (_col == null)
                _col = GetComponent<TriggerTrackerComponent>();

            _col.RegisterReceiver(this);
        }

        private void OnDisable()
        {
            if (_col != null)
                _col.UnregisterReceiver(this);
        }

        public void Setup(BaseGameEntityComponent owner, SerializedProjectileBehavior behavior, ProjectileHitRules rules)
        {
            _owner = owner;
            _rules = rules;
            _behavior = behavior.Deserialize(owner);
        }

        public void TriggerEntered(TriggerHitInfo hit)
        {
            if (_finished) return;

            var collision = Evaluate(hit);

            // 1. эффекты попадания (события спавнера -> урон, звук, частицы)
            ProjectileHit.Invoke(this, collision);
            _receiver?.TriggerEntered(hit);
            if (_finished) return;

            // 2. реакция поведения: отскок, возврат, разрушение о стену, исчерпание лимита
            if (_behavior.OnCollision(collision) == ProjectileCollisionResult.Finish)
            {
                Finish(collision.IsEnvironment ? ProjectileFinishReason.Environment
                    : collision.BudgetExhausted ? ProjectileFinishReason.HitsExhausted
                    : ProjectileFinishReason.Behavior);
                return;
            }

            if (_behavior.BehaviorCompleted) Finish(ProjectileFinishReason.Behavior);
        }

        /// <summary>
        /// Единая точка разбора попадания: стена / владелец / цель, лимит и повторы.
        /// </summary>
        private ProjectileCollision Evaluate(TriggerHitInfo hit)
        {
            bool hasTarget = hit.TryGetEntityTarget(out var target) && hit.LayerKind != HitLayerKind.Invalid;
            if (!hasTarget)
                return new ProjectileCollision(transform, hit, null, true, false, false, false);

            if (target == _owner)
                return new ProjectileCollision(transform, hit, target, false, true, false, false);

            bool hasBudget = _rules.maxTargets <= 0 || _hitsDone < _rules.maxTargets;
            bool accepted = hasBudget && (_rules.allowRepeatHits || _hitTargets.Add(target));

            bool exhausted = false;
            if (accepted)
            {
                _hitsDone++;
                exhausted = _rules.maxTargets > 0 && _hitsDone >= _rules.maxTargets;
            }

            return new ProjectileCollision(transform, hit, target, false, false, accepted, exhausted);
        }

        public void TriggerExited(TriggerHitInfo triggerExitInfo) => _receiver?.TriggerExited(triggerExitInfo);

        void Update()
        {
            if (Paused || _finished) return;
            _behavior.UpdatePosition(Time.deltaTime, transform);
            if (_behavior.BehaviorCompleted) Finish(ProjectileFinishReason.Expired);
        }

        /// <summary>
        /// Единственная точка завершения жизни. Идемпотентна: стена и истечение срока
        /// в одном кадре не приведут к двойному Release в пуле.
        /// </summary>
        public void Finish(ProjectileFinishReason reason)
        {
            if (_finished) return;
            _finished = true;
            LastFinishReason = reason;
            ProjectileFinished.Invoke(this, reason);
        }

        public void Cancel() => Finish(ProjectileFinishReason.Cancelled);

        public bool Paused { get; set; } = false;

        public void Reset()
        {
            _finished = false;
            _hitsDone = 0;
            _hitTargets.Clear();
            _behavior.Reset();
            transform.position = Vector3.zero;
        }

        /// <summary>
        /// called by the spawner (hit producer)
        /// </summary>
        public bool Active
        {
            get => _col.Active;
            set
            {
                _col.Active = value;
                foreach (var part in _parts)
                {
                    part.TriggerState(value ? StateMachineNotifyType.Use : StateMachineNotifyType.EndUse);
                }
            }
        }

        public void RegisterReceiver(ITriggerNotificationReceiver receiver) => _receiver = receiver;
        public void UnregisterReceiver(ITriggerNotificationReceiver receiver) => _receiver = null;

        public void AreaCast(ITriggerNotificationReceiver receiver)
        {
            // noop
        }

        public void OnChangeUsableState(StateMachineNotifyType notification)
        {
            // this is not called in current implementation
        }
    }
}
