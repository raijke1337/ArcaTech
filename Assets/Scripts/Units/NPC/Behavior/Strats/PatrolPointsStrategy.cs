using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Arcatech.Units.Behaviour.Node;

namespace Arcatech.Units.Behaviour
{

    public class PatrolPointsStrategy : IBehaviorTreeStrategy
    {
        readonly Transform entity;
        readonly NavMeshAgent agent;
        readonly List<Transform> patrolPoints;
        int currentIndex;
        bool isPathCalculated;

        readonly CountDownTimer idleTimer;

        public PatrolPointsStrategy(Transform entity, NavMeshAgent agent, List<Transform> patrolPoints, float waitTime = 2f)
        {
            this.entity = entity;
            this.agent = agent;
            this.patrolPoints = patrolPoints;
            idleTimer = new CountDownTimer(waitTime);
            currentIndex = 0;
        }

        public NodeStatus Process(NPCUnitComponent actor)
        {
            if (idleTimer.IsRunning) // currentlly idling
            {
                idleTimer.Tick(Time.deltaTime);
                return NodeStatus.Running;
            }
            var target = patrolPoints[currentIndex];
            agent.SetDestination(target.position);

            if (!agent.pathPending)
            {
                isPathCalculated = true;
            }

            if (isPathCalculated && agent.remainingDistance < 1f)
            {
                idleTimer.Reset();
                idleTimer.Start();

                currentIndex++;
                if (currentIndex >= patrolPoints.Count) { currentIndex = 0; }
                isPathCalculated = false;
            }

            return NodeStatus.Running;
        }

        public void Reset()
        {
            currentIndex = 0;
            isPathCalculated = false;
            idleTimer.Reset();
        }
    }
}