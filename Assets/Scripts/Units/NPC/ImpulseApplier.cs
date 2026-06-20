using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace Arcatech.Units
{
    [RequireComponent(typeof(Rigidbody), typeof(NPCBehaviorWrapper))]

    public sealed class ImpulseApplier : MonoBehaviour, IPausableComponent
    {
        private Rigidbody _rigidbody;
        private NavMeshAgent agent;
        private BehaviorGraphAgent behavior;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            agent = GetComponent<NavMeshAgent>();
            behavior = GetComponent<BehaviorGraphAgent>();
        }


        public bool Paused { get; set; }

        [Space, Header("Impulse Settings")]
        [Tooltip("Горизонтальная скорость (м/с), сообщаемая импульсом ±1 (додж/отскок).")]
        [SerializeField]
        private float _impulseSpeed = 8f;

        [Tooltip("Длительность импульса (сек). Агент включается обратно по истечении или при почти полной остановке.")]
        [SerializeField]
        private float _impulseDuration = 0.3f;

        [Tooltip("Скорость (м/с), ниже которой импульс завершается досрочно.")] [SerializeField]
        private float _impulseEndSpeed = 0.5f;

        [Tooltip("Радиус поиска ближайшей точки NavMesh при восстановлении агента после импульса.")] [SerializeField]
        private float _warpSampleDistance = 2f;

// === Runtime state для handoff agent ↔ rigidbody ===
        private bool _impulseActive;
        private float _impulseTimeRemaining;
        private bool _wasAgentEnabled;
        private bool _wasUpdatePosition;
        private bool _wasUpdateRotation;
        private bool _wasRigidbodyUseGravity;

        public void ApplyImpulse(Vector3 impulse)
        {
            BeginImpulse(impulse);
        }

        public void ApplyImpulse(float impulseRelative)
        {
            float t = Mathf.Clamp(impulseRelative, -1f, 1f);
            Vector3 worldVelocity = transform.forward * (t * _impulseSpeed);
            BeginImpulse(worldVelocity);
        }

        private void BeginImpulse(Vector3 worldVelocity)
        {
            if (_impulseActive)
            {
                EndImpulse();
            }

            // Кешируем то, что собираемся временно переопределить
            _wasAgentEnabled = agent.enabled;
            _wasUpdatePosition = agent.updatePosition;
            _wasUpdateRotation = agent.updateRotation;
            _wasRigidbodyUseGravity = _rigidbody.useGravity;

            // --- Отключаем агент ---
            if (_wasAgentEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.updatePosition = false;
                agent.updateRotation = false;
                agent.enabled = false;
            }

            // --- Передаём управление rigidbody ---
            _rigidbody.useGravity = true;
            SetRigidbodyVelocity(worldVelocity);

            _impulseActive = true;
            _impulseTimeRemaining = _impulseDuration;
        }

        private void LateUpdate()
        {

            if (!_impulseActive) return;

            _impulseTimeRemaining -= Time.deltaTime;

            float speed = GetRigidbodySpeed();
            bool willEndThisFrame = _impulseTimeRemaining <= 0f || speed < _impulseEndSpeed;

            if (willEndThisFrame)
            {
                EndImpulse();
            }
        }


        private void EndImpulse()
        {
            if (!_impulseActive)
            {
                return;
            }

            _impulseActive = false;

            // Гасим rigidbody
            SetRigidbodyVelocity(Vector3.zero);
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.useGravity = _wasRigidbodyUseGravity;

            if (!_wasAgentEnabled)
            {
                return;
            }

            // Включаем агент и ищем ближайшую точку навмеша
            agent.enabled = true;
          //  behavior.enabled = true;

            Vector3 warpTarget = transform.position;
            bool sampleOk = NavMesh.SamplePosition(transform.position, out NavMeshHit hit, _warpSampleDistance,
                NavMesh.AllAreas);
            if (sampleOk)
            {
                warpTarget = hit.position;
            }

            bool warpResult = agent.Warp(warpTarget);

            agent.updatePosition = _wasUpdatePosition;
            agent.updateRotation = _wasUpdateRotation;
            bool shouldStop = Paused;
            agent.isStopped = shouldStop;

        }
        
        /// <summary>
        /// Обёртка для совместимости с Unity 6+ (linearVelocity) и более старыми версиями (velocity).
        /// </summary>
        private void SetRigidbodyVelocity(Vector3 v)
        {
#if UNITY_6000_0_OR_NEWER
            _rigidbody.linearVelocity = v;
#else
    _rigidbody.velocity = v;
#endif
        }

        private float GetRigidbodySpeed()
        {
#if UNITY_6000_0_OR_NEWER
            return _rigidbody.linearVelocity.magnitude;
#else
    return _rigidbody.velocity.magnitude;
#endif
        }
    }
}