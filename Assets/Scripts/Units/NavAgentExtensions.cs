using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Запросы к навмешу С УЧЁТОМ типа агента (Bot, Humanoid...) и его areaMask.
/// Перегрузки NavMesh.SamplePosition(..., int areaMask) / CalculatePath(..., int areaMask)
/// ищут только в навмеше типа по умолчанию (Humanoid, id 0), поэтому для агента "Bot"
/// они дают точки, на которых этот агент стоять не может.
/// </summary>
public static class NavAgentExtensions
{
    public static NavMeshQueryFilter QueryFilter(this NavMeshAgent agent)
        => new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };

    public static bool SampleOwnMesh(this NavMeshAgent agent, Vector3 position, float maxDistance, out NavMeshHit hit)
        => NavMesh.SamplePosition(position, out hit, maxDistance, agent.QueryFilter());

    public static bool CalculateOwnPath(this NavMeshAgent agent, Vector3 from, Vector3 to, NavMeshPath path)
        => NavMesh.CalculatePath(from, to, agent.QueryFilter(), path);
}
