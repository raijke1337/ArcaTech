using Arcatech.Units;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody), typeof(NPCBehaviorWrapper))]
[DisallowMultipleComponent]
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

    public bool IsActive => _impulseActive;
    public bool Paused { get; set; }

    [Space, Header("Impulse Settings")]
    [SerializeField]
    private float _impulseSpeed = 8f;

    [SerializeField]
    private float _impulseDuration = 0.3f;

    [SerializeField]
    private float _impulseEndSpeed = 0.5f;

    [SerializeField]
    private float _warpSampleDistance = 2f;
    
    [SerializeField]
    [Tooltip("Увеличенный радиус для поиска точки, если первая попытка не удалась")]
    private float _warpSampleDistanceExtended = 5f;

    private bool _impulseActive;
    private float _impulseTimeRemaining;
    private bool _wasAgentEnabled;
    private bool _wasUpdatePosition;
    private bool _wasUpdateRotation;
    private bool _wasRigidbodyUseGravity;
    private Vector3 _lastKnownValidNavMeshPosition;

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

        // Сохраняем последнюю известную валидную позицию на NavMesh
        if (agent.enabled && agent.isOnNavMesh)
        {
            _lastKnownValidNavMeshPosition = agent.transform.position;
        }

        _wasAgentEnabled = agent.enabled;
        _wasUpdatePosition = agent.updatePosition;
        _wasUpdateRotation = agent.updateRotation;
        _wasRigidbodyUseGravity = _rigidbody.useGravity;

        if (_wasAgentEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.updatePosition = false;
            agent.updateRotation = false;
            agent.enabled = false;
        }

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

        SetRigidbodyVelocity(Vector3.zero);
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.useGravity = _wasRigidbodyUseGravity;

        if (!_wasAgentEnabled)
        {
            return;
        }

        agent.enabled = true;

        if (!TryWarpAgentToNavMesh())
        {
            Debug.LogWarning($"[ImpulseApplier] Не удалось вернуть {gameObject.name} на NavMesh. " +
                $"Агент отключен.", gameObject);
            agent.enabled = false;
            return;
        }

        agent.updatePosition = _wasUpdatePosition;
        agent.updateRotation = _wasUpdateRotation;
        agent.isStopped = Paused;
    }

    /// <summary>
    /// Пытается переместить агента на NavMesh с увеличивающимся радиусом поиска
    /// </summary>
    private bool TryWarpAgentToNavMesh()
    {
        Vector3 currentPos = transform.position;

        // Попытка 1: стандартный радиус от текущей позиции
        if (NavMesh.SamplePosition(currentPos, out NavMeshHit hit, _warpSampleDistance, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            return true;
        }

        // Попытка 2: увеличенный радиус от текущей позиции
        if (NavMesh.SamplePosition(currentPos, out hit, _warpSampleDistanceExtended, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            Debug.LogWarning($"[ImpulseApplier] {gameObject.name} требовал расширенного поиска NavMesh", gameObject);
            return true;
        }

        // Попытка 3: от последней известной валидной позиции
        if (NavMesh.SamplePosition(_lastKnownValidNavMeshPosition, out hit, _warpSampleDistance, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            Debug.LogWarning($"[ImpulseApplier] {gameObject.name} возвращен на последнюю валидную позицию", gameObject);
            return true;
        }

        return false;
    }

    private void SetRigidbodyVelocity(Vector3 v)
    {
        _rigidbody.linearVelocity = v;
    }

    private float GetRigidbodySpeed()
    {
        return _rigidbody.linearVelocity.magnitude;
    }
}