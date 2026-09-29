using System;
using Arcatech.Units;
using Arcatech.Units.Control;
using UnityEngine;
using UnityEngine.AI;

namespace Arcatech.Units.Control
{
    /// <summary>
    /// Отталкивания и рывки для NPC на NavMeshAgent.
    ///
    /// Rigidbody ВСЕГДА kinematic, гравитацию физика не считает: позицией владеет либо NavMeshAgent,
    /// либо этот скрипт. Фазы:
    ///
    ///  Sliding  - юнит на навмеше. Агент остаётся включённым (путь не теряется), двигаем через agent.Move():
    ///             навмеш физически не даёт выйти за край. Стены режем CapsuleCast'ом.
    ///  Physical - юнит покинул навмеш (толчок с CanLeaveNavMesh, подбросило, край карты). Агент выключен,
    ///             гравитация + скольжение по коллайдерам считаем сами. Никакой телепортации по пути.
    ///  Stranded - приземлился и остановился там, где навмеша нет ближе _reattachDistance. Стоит на месте,
    ///             через _strandedTimeout вызывается событие Stranded (убить / оставить решаете вы).
    ///
    /// Возврат на навмеш - только после приземления и остановки, и только в пределах _reattachDistance
    /// (по умолчанию ~1 м: компенсирует отступ навмеша от края пола). Дальше не телепортируем.
    /// Падение ниже _fallKillDepth от последней точки на навмеше вызывает FellOutOfWorld.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(NavMeshAgent), typeof(NPCBehaviorWrapper))]
    [DisallowMultipleComponent]
    public sealed class ImpulseApplier : MonoBehaviour, IPausableComponent
    {
        public enum Phase
        {
            Idle,
            Sliding,
            Physical,
            Stranded
        }

        // ---- имена _impulseDuration / _impulseEndSpeed сохранены, чтобы не потерять значения на префабах ----
        [Header("Slide (пока юнит на NavMesh)")] [SerializeField, Tooltip("Длительность толчка по умолчанию, с.")]
        private float _impulseDuration = 0.3f;

        [SerializeField, Tooltip("Толчок заканчивается, когда скорость упала ниже этого значения, м/с.")]
        private float _impulseEndSpeed = 0.5f;

        [SerializeField, Min(0f),
         Tooltip(
             "Экспоненциальное затухание скорости на навмеше, 1/с. 0 = постоянная скорость (как раньше). 4-8 даёт мягкое торможение.")]
        private float _slideDrag = 0f;

        [SerializeField, Min(0.1f), Tooltip("Ограничение скорости толчка, м/с (защита от кривых данных).")]
        private float _maxSpeed = 30f;

        [Header("Physical (вне NavMesh)")] [SerializeField, Tooltip("Множитель гравитации при полёте.")]
        private float _gravityScale = 1f;

        [SerializeField, Min(0f), Tooltip("Торможение по земле вне навмеша, 1/с.")]
        private float _groundDrag = 6f;

        [SerializeField, Min(0f), Tooltip("Торможение в воздухе, 1/с.")]
        private float _airDrag = 0f;

        [SerializeField,
         Tooltip("С чем сталкивается тело во время толчка (окружение). Слой самого юнита исключается автоматически.")]
        private LayerMask _obstacleMask = ~0;

        [SerializeField, Min(0f), Tooltip("Радиус капсулы для столкновений. 0 = радиус NavMeshAgent.")]
        private float _collisionRadius = 0f;

        [Header("Возврат на NavMesh")]
        [SerializeField, Min(0f),
         Tooltip(
             "Макс. расстояние от точки остановки до навмеша, при котором агент возвращается на него (короткая коррекция, не телепорт). 0 = никогда не возвращать.")]
        private float _reattachDistance = 1f;

        [SerializeField, Min(0f), Tooltip("Через сколько секунд стояния вне навмеша вызвать событие Stranded.")]
        private float _strandedTimeout = 3f;

        [SerializeField, Min(1f),
         Tooltip(
             "На сколько метров ниже последней точки на навмеше считается «упал за карту» (событие FellOutOfWorld).")]
        private float _fallKillDepth = 10f;

        // ---- события ----
        /// <summary>Толчок закончился, агент снова рулит юнитом.</summary>
        public event Action<ImpulseApplier> MotionEnded;

        /// <summary>Юнит стоит вне навмеша и вернуться некуда. Подпишите сюда убийство/деспавн, если нужно.</summary>
        public event Action<ImpulseApplier> Stranded;

        /// <summary>Юнит упал глубже _fallKillDepth. Подпишите сюда убийство.</summary>
        public event Action<ImpulseApplier> FellOutOfWorld;

        public bool Paused { get; set; }
        public Phase Current => _phase;

        /// <summary>true, пока юнита двигает не NavMeshAgent (любая фаза кроме Idle).</summary>
        public bool IsActive => _phase != Phase.Idle;

        private const float Lift = 0.03f; // капсула приподнята над ногами, чтобы стоять на земле без «залипания»
        private const float Skin = 0.03f;

        private Rigidbody _rb;
        private NavMeshAgent _agent;
        private int _mask;

        private Phase _phase;
        private Vector3 _velocity;
        private float _timeLeft;
        private bool _canLeave;
        private bool _grounded;
        private float _lastNavY;
        private float _strandedTimer;
        private bool _strandedRaised;
        private bool _fellRaised;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _agent = GetComponent<NavMeshAgent>();
            _mask = _obstacleMask.value & ~(1 << gameObject.layer);

            // Позицией владеет агент / этот скрипт. Динамический Rigidbody с гравитацией и агент дерутся за transform.
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }

        // =========================================================================================
        //  API
        // =========================================================================================

        public void Apply(in MotionRequest request)
        {
            if (Paused) return;

            Vector3 v = request.Velocity;
            if (request.Kind == MotionKind.Dash) v.y = 0f;
            if (v.sqrMagnitude < 1e-4f) return;
            v = Vector3.ClampMagnitude(v, _maxSpeed);

            _velocity = v;
            _timeLeft = request.Duration > 0f ? request.Duration : _impulseDuration;
            _canLeave = request.Kind == MotionKind.Knockback && request.CanLeaveNavMesh;
            _strandedRaised = false;
            _strandedTimer = 0f;

            if (_phase == Phase.Idle)
            {
                _lastNavY = transform.position.y;
                if (OnNavMesh)
                {
                    _agent.isStopped = true; // путь сохраняется, агент просто не идёт сам
                    _agent.velocity = Vector3.zero;
                }
            }

            bool launched = v.y > 0.1f;
            if (launched || !OnNavMesh) EnterPhysical();
            else _phase = Phase.Sliding;
        }

        // =========================================================================================
        //  Tick
        // =========================================================================================

        private bool OnNavMesh => _agent.enabled && _agent.isOnNavMesh;

        private void Update()
        {
            if (_phase == Phase.Idle || Paused) return;

            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            if (_phase == Phase.Sliding) TickSliding(dt);
            else TickPhysical(dt);
        }

        private void TickSliding(float dt)
        {
            if (!OnNavMesh) // навмеш пропал из-под ног (динамический obstacle, перепекание)
            {
                EnterPhysical();
                return;
            }

            if (_slideDrag > 0f) _velocity *= Mathf.Exp(-_slideDrag * dt);
            _timeLeft -= dt;

            Vector3 pos = transform.position;
            Vector3 step = new Vector3(_velocity.x, 0f, _velocity.z) * dt;
            step = ClipByObstacles(pos, step);

            // Root motion выключает updatePosition: transform уехал от внутренней позиции агента - синхронизируем.
            if (!_agent.updatePosition) _agent.nextPosition = transform.position;

            // Край навмеша на пути: либо выбрасываем за край, либо agent.Move() упрёт нас в него.
            if (_canLeave && _agent.Raycast(pos + step, out _))
            {
                EnterPhysical();
                return;
            }

            _agent.Move(step);
            if (!_agent.updatePosition) transform.position = _agent.nextPosition;
            _lastNavY = transform.position.y;

            float horizontalSpeed = new Vector2(_velocity.x, _velocity.z).magnitude;
            if (_timeLeft <= 0f || horizontalSpeed < _impulseEndSpeed) EndMotion();
        }

        private void TickPhysical(float dt)
        {
            _velocity.y -= Mathf.Abs(Physics.gravity.y) * _gravityScale * dt;

            float drag = _grounded ? _groundDrag : _airDrag;
            if (drag > 0f)
            {
                float k = Mathf.Exp(-drag * dt);
                _velocity.x *= k;
                _velocity.z *= k;
            }

            Vector3 pos = MoveAndSlide(transform.position, _velocity * dt);
            transform.position = pos;

            if (pos.y < _lastNavY - _fallKillDepth)
            {
                if (!_fellRaised)
                {
                    _fellRaised = true;
                    if (FellOutOfWorld == null)
                        Debug.LogWarning(
                            $"[ImpulseApplier] {name} упал за карту, но на FellOutOfWorld никто не подписан.", this);
                    FellOutOfWorld?.Invoke(this);
                }

                return;
            }

            if (!_grounded) return;

            float horizontalSpeed = new Vector2(_velocity.x, _velocity.z).magnitude;
            if (horizontalSpeed < _impulseEndSpeed) Settle();
        }

        /// <summary>Юнит стоит на земле вне навмеша: вернуть на навмеш (коротко) или пометить как Stranded.</summary>
        private void Settle()
        {
            if (_reattachDistance > 0f &&
                _agent.SampleOwnMesh(transform.position, _reattachDistance, out NavMeshHit hit))
            {
                Reattach(hit.position);
                return;
            }

            if (_phase != Phase.Stranded)
            {
                _phase = Phase.Stranded;
                _strandedTimer = 0f;
            }

            _velocity.x = 0f;
            _velocity.z = 0f;
            _strandedTimer += Time.deltaTime;

            if (!_strandedRaised && _strandedTimer >= _strandedTimeout)
            {
                _strandedRaised = true;
                Stranded?.Invoke(this);
            }
        }

        // =========================================================================================
        //  Переходы
        // =========================================================================================

        private void EnterPhysical()
        {
            if (_agent.enabled)
            {
                if (_agent.isOnNavMesh)
                {
                    _agent.isStopped = true;
                    _agent.velocity = Vector3.zero;
                }

                _agent.enabled = false;
            }

            _phase = Phase.Physical;
            _grounded = false;
        }

        private void Reattach(Vector3 navPosition)
        {
            transform.position = navPosition;
            _agent.enabled = true;
            if (!_agent.isOnNavMesh) _agent.Warp(navPosition);

            if (!_agent.isOnNavMesh)
            {
                // навмеш «моргнул» между запросом и включением - остаёмся физическим телом, попробуем на следующем кадре
                _agent.enabled = false;
                return;
            }

            _fellRaised = false;
            EndMotion();
        }

        private void EndMotion()
        {
            _velocity = Vector3.zero;
            _phase = Phase.Idle;
            if (OnNavMesh) _agent.velocity = Vector3.zero;
            MotionEnded?.Invoke(this); // NPCBehaviorWrapper вернёт agent.isStopped в нужное значение
        }

        // =========================================================================================
        //  Коллизии (kinematic-движение через CapsuleCast, без Rigidbody-физики)
        // =========================================================================================

        private void GetCapsule(Vector3 pos, out Vector3 bottom, out Vector3 top, out float radius)
        {
            radius = Mathf.Max(0.05f, _collisionRadius > 0f ? _collisionRadius : _agent.radius);
            float height = Mathf.Max(_agent.height, radius * 2f);
            bottom = pos + Vector3.up * (radius + Lift);
            top = pos + Vector3.up * (height - radius + Lift);
        }

        /// <summary>Режем горизонтальный шаг по стенам. Пологие поверхности (пол, рампы) игнорируем - за проходимость отвечает навмеш.</summary>
        private Vector3 ClipByObstacles(Vector3 pos, Vector3 step)
        {
            float dist = step.magnitude;
            if (dist < 1e-5f) return step;

            GetCapsule(pos, out Vector3 a, out Vector3 b, out float r);
            Vector3 dir = step / dist;

            if (Physics.CapsuleCast(a, b, r, dir, out RaycastHit hit, dist + Skin, _mask,
                    QueryTriggerInteraction.Ignore)
                && hit.normal.y <= 0.5f)
            {
                Vector3 n = new Vector3(hit.normal.x, 0f, hit.normal.z);
                if (n.sqrMagnitude > 1e-6f)
                {
                    n.Normalize();
                    float into = Vector3.Dot(_velocity, n);
                    if (into < 0f) _velocity -= n * into; // гасим составляющую скорости в стену
                }

                return dir * Mathf.Max(0f, hit.distance - Skin);
            }

            return step;
        }

        /// <summary>Движение с гравитацией: до 3 итераций скольжения вдоль поверхностей. Обновляет _velocity и _grounded.</summary>
        private Vector3 MoveAndSlide(Vector3 pos, Vector3 delta)
        {
            _grounded = false;

            for (int i = 0; i < 3 && delta.sqrMagnitude > 1e-8f; i++)
            {
                float dist = delta.magnitude;
                Vector3 dir = delta / dist;
                GetCapsule(pos, out Vector3 a, out Vector3 b, out float r);

                if (!Physics.CapsuleCast(a, b, r, dir, out RaycastHit hit, dist + Skin, _mask,
                        QueryTriggerInteraction.Ignore))
                {
                    pos += delta;
                    break;
                }

                float move = Mathf.Max(0f, hit.distance - Skin);
                pos += dir * move;
                Vector3 remaining = delta - dir * move;

                if (hit.normal.y > 0.6f)
                {
                    _grounded = true;
                    if (_velocity.y < 0f) _velocity.y = 0f;
                }

                float into = Vector3.Dot(_velocity, hit.normal);
                if (into < 0f) _velocity -= hit.normal * into;

                delta = Vector3.ProjectOnPlane(remaining, hit.normal);
            }

            return pos;
        }

        private void OnDrawGizmosSelected()
        {
            if (_agent == null) _agent = GetComponent<NavMeshAgent>();
            if (_agent == null) return;
            GetCapsule(transform.position, out Vector3 a, out Vector3 b, out float r);
            Gizmos.color = _phase == Phase.Idle ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(a, r);
            Gizmos.DrawWireSphere(b, r);
        }
    }
}