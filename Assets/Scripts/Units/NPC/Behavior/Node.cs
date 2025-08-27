using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Arcatech.Units.Behaviour
{

    /// <summary>
    /// possible children:
    /// Untilsuccess
    /// Repeat n times
    /// </summary>
    public abstract class Node  // node is the base building block of the behavior tree
    {

        public enum NodeStatus { Success, Fail, Running };
        public readonly string NodeName;
        public readonly int Priority;
        public readonly List<Node> Children = new();
        protected int currentChildIndex = 0;


        public Node (string name = "Node",int priority = 0)
        {
            NodeName = name;
            Priority = priority;
        }

        public void AddChild (Node c) => Children.Add(c);
        public virtual NodeStatus Process(NPCUnitComponent actor)
        {
            return Children[currentChildIndex].Process(actor);
        }
        public virtual void Reset() 
        { 
            currentChildIndex = 0;
            foreach (Node c in Children)
            {
                c.Reset();
            }
        }
    }

}