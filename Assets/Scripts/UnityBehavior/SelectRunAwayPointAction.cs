using Arcatech.Units;
using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Select Run Away Point", story: "[Agent] find a [point] away from [Player] within [Range]", category: "Action/Game/GemDron", id: "c2192b73d1c4ed84d6f016ab5f214d6c")]
public partial class SelectRunAwayPointAction : Action
{
    [SerializeReference] public BlackboardVariable<NPCBehaviorWrapper> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> Point;
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BlackboardVariable<float> Range;

    [Tooltip("Количество попыток поиска точки")]
    private const int MAX_ATTEMPTS = 16;
    
    [Tooltip("Минимальное расстояние от игрока")]
    private const float MIN_DISTANCE_FROM_PLAYER = 5f;

    protected override Status OnStart()
    {
        // Валидация входных данных
        if (Agent.Value == null || Agent.Value.transform == null)
        {
            Debug.LogError("[SelectRunAwayPoint] Agent is null or doesn't have transform!");
            return Status.Failure;
        }

        if (Player.Value == null)
        {
            Debug.LogError("[SelectRunAwayPoint] Player is null!");
            return Status.Failure;
        }

        if (Range.Value <= 0)
        {
            Debug.LogWarning("[SelectRunAwayPoint] Range must be greater than 0. Using default: 20");
            Range.Value = 20f;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Vector3 agentPosition = Agent.Value.transform.position;
        Vector3 playerPosition = Player.Value.transform.position;
        float searchRange = Range.Value;

        Vector3 bestPoint = Vector3.zero;
        float maxDistance = 0f;
        bool foundValidPoint = false;

        // Направление от игрока к агенту (базовое направление бегства)
        Vector3 fleeDirection = (agentPosition - playerPosition).normalized;

        // Метод 1: Пробуем найти точку в противоположном от игрока направлении
        for (int i = 0; i < MAX_ATTEMPTS; i++)
        {
            // Генерируем случайную точку в конусе от игрока
            float angle = UnityEngine.Random.Range(-60f, 60f); // Конус ±60 градусов
            float distance = UnityEngine.Random.Range(searchRange * 0.5f, searchRange);
            
            Vector3 randomDirection = Quaternion.Euler(0, angle, 0) * fleeDirection;
            Vector3 candidatePoint = agentPosition + randomDirection * distance;

            // Проверяем, что точка на NavMesh
            if (NavMesh.SamplePosition(candidatePoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                // Проверяем, что точка достижима
                NavMeshPath path = new NavMeshPath();
                if (NavMesh.CalculatePath(agentPosition, hit.position, NavMesh.AllAreas, path))
                {
                    if (path.status == NavMeshPathStatus.PathComplete)
                    {
                        // Вычисляем расстояние от точки до игрока
                        float distanceFromPlayer = Vector3.Distance(hit.position, playerPosition);
                        
                        // Проверяем, что точка дальше от игрока, чем текущая позиция агента
                        if (distanceFromPlayer > MIN_DISTANCE_FROM_PLAYER && distanceFromPlayer > maxDistance)
                        {
                            maxDistance = distanceFromPlayer;
                            bestPoint = hit.position;
                            foundValidPoint = true;
                        }
                    }
                }
            }
        }

        // Метод 2: Если не нашли точку, пробуем радиальный поиск
        if (!foundValidPoint)
        {
            for (int i = 0; i < MAX_ATTEMPTS / 2; i++)
            {
                // Генерируем случайную точку вокруг агента
                Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * searchRange;
                Vector3 candidatePoint = agentPosition + new Vector3(randomCircle.x, 0, randomCircle.y);

                if (NavMesh.SamplePosition(candidatePoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                {
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(agentPosition, hit.position, NavMesh.AllAreas, path))
                    {
                        if (path.status == NavMeshPathStatus.PathComplete)
                        {
                            float distanceFromPlayer = Vector3.Distance(hit.position, playerPosition);
                            float currentDistanceFromPlayer = Vector3.Distance(agentPosition, playerPosition);
                            
                            // Выбираем точку, которая дальше от игрока, чем текущая позиция
                            if (distanceFromPlayer > currentDistanceFromPlayer && distanceFromPlayer > maxDistance)
                            {
                                maxDistance = distanceFromPlayer;
                                bestPoint = hit.position;
                                foundValidPoint = true;
                            }
                        }
                    }
                }
            }
        }

        // Результат
        if (foundValidPoint)
        {
            Point.Value = bestPoint;
            
            #if UNITY_EDITOR
          //  Debug.Log($"[SelectRunAwayPoint] Found escape point at distance {maxDistance:F2}m from player");
            Debug.DrawLine(agentPosition, bestPoint, Color.cyan, 2f);
            Debug.DrawLine(playerPosition, bestPoint, Color.yellow, 2f);
            #endif
            
            return Status.Success;
        }
        else
        {
            Debug.LogWarning($"[SelectRunAwayPoint] Failed to find valid escape point within range {searchRange}");
            
            // Fallback: используем текущую позицию агента
            Point.Value = agentPosition;
            return Status.Failure;
        }
    }

    protected override void OnEnd()
    {
        // Очистка ресурсов (если необходимо)
    }

    #if UNITY_EDITOR
    // Визуализация в редакторе (опционально)
    private void OnDrawGizmos()
    {
        if (Agent?.Value != null && Player?.Value != null && Range.Value > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Agent.Value.transform.position, Range.Value);
            
            if (Point.Value != Vector3.zero)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(Point.Value, 0.5f);
            }
        }
    }
    #endif
}