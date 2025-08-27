using Arcatech.Units.Inputs;
using System;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;


namespace Arcatech.Units.Behaviour
{
    public class BehaviourAction : IBehaviorTreeStrategy
    {

        public BehaviourAction(Action act) => this.act = act;

        readonly Action act;

        public Node.NodeStatus Process(NPCUnitComponent actor)
        {
            act();
            return Node.NodeStatus.Success;
        }

        public void Reset()
        {
            
        }
    }

}