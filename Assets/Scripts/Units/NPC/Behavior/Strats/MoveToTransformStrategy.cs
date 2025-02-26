using UnityEngine;
using UnityEngine.AI;
using static Arcatech.Units.Behaviour.Node;
using static Unity.Burst.Intrinsics.X86;

namespace Arcatech.Units.Behaviour
{

    public class MoveToTransformStrategy : IBehaviorTreeStrategy
    {
        readonly NavMeshAgent nma;
        readonly Transform target;
        bool isFollowingCalculatedPath;
        readonly bool requireAngle; // if angle is needed, rotate first, then start movement
        readonly float angle;

        public MoveToTransformStrategy(NavMeshAgent nma, Transform target, bool requireAngle = false, float angle = 15f)
        {
            this.nma = nma;
            this.target = target;
            this.requireAngle = requireAngle;
            this.angle = angle;
        }

        public NodeStatus Process(ControlledUnit actor)
        {
            Vector3 tgt = target.position;
            if (isFollowingCalculatedPath && nma.remainingDistance < nma.stoppingDistance)
            {
                if (!requireAngle)
                {
                    Reset();
                    return NodeStatus.Success;
                }
                else
                {
                    Vector3 desired = (tgt - actor.transform.position).normalized;
                    float currentangle = Vector3.Angle(actor.transform.forward, desired);
                    if (currentangle < angle)
                    {
                        return NodeStatus.Success;
                    }
                    else
                    {
                        actor.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(actor.transform.forward, desired, 0.01f, 0f));
                        return NodeStatus.Running;
                    }
                }
            }
            nma.SetDestination(tgt);
            if (requireAngle && !isFollowingCalculatedPath)
            {
                // check if angle between forward and target is small enough to start moving
                Vector3 desired = (tgt - actor.transform.position).normalized;
                float currentangle = Vector3.Angle(actor.transform.forward, desired);
                if (currentangle < angle)
                {
                    nma.isStopped = false;
                }
                else
                {
                    nma.isStopped = true;
                    actor.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(actor.transform.forward, desired, 0.01f, 0f));
                }
            }
            if (!nma.pathPending)
            {
                isFollowingCalculatedPath = true;
            }
            return NodeStatus.Running;
        }

        public void Reset() => isFollowingCalculatedPath = false;
    }
}