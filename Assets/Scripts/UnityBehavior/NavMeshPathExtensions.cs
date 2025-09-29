using UnityEngine;
using UnityEngine.AI;

public static class NavMeshPathExtensions
{
    public static float GetPathLength(this NavMeshPath path)
    {
        var pathPoints = path.corners;
        
        if (pathPoints == null || pathPoints.Length < 2)
            return 0f;
        
        float totalLength = 0f;
        
        for (int i = 1; i < pathPoints.Length; i++)
        {
            float dx = pathPoints[i].x - pathPoints[i - 1].x;
            float dy = pathPoints[i].y - pathPoints[i - 1].y;
            float dz = pathPoints[i].z - pathPoints[i - 1].z;
            
            totalLength += Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
        }
        
        return totalLength;
    }
}