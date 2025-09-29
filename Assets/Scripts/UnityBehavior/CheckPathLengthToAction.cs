using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;

namespace Unity.Behavior

{

    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Check Path Length To", story: "[Agent] compares path length to [Target] with [MaxPathLength]",
        category: "Flow/Conditional", id: "a6302b6b96abf1d319fda575e6a602ed")]
    public partial class CheckPathLengthToAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<Transform> Target;
        [SerializeReference] public BlackboardVariable<float> MaxPathLength;

        private NavMeshAgent agent;
        NavMeshPath path;
        
        protected override Status OnStart()
        {
            if (Agent.Value.TryGetComponent(out agent))
            {
                agent.ResetPath();
                return Status.Running;
            }
            else return Status.Failure;
        }

        protected override Status OnUpdate()
        {
            if (!agent.hasPath)
            {
                if (!agent.pathPending)
                {
                    agent.CalculatePath(Target.Value.transform.position, path);
                }
                return Status.Running;
            }
            else
            {
                return path.GetPathLength() < MaxPathLength.Value ? Status.Success : Status.Failure;
            }
        }

        protected override void OnEnd()
        {
            agent.ResetPath();
        }
    }
}