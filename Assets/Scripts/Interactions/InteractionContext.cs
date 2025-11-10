using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class InteractionContext : IInteractionContext
    {
        public string SomeInformation { get; private set; }
        public EntityStateMachineComponent EntityStateMachineComponent { get; }
        public Transform ActionTransform { get; }
        
        public static InteractionContext Create(EntityStateMachineComponent actor, Transform place,string text = "undefined context")
        {
            return new InteractionContext(actor,place,text); 
        }
        

        private InteractionContext(EntityStateMachineComponent actor, Transform place, string someInformation)
        {
            SomeInformation = someInformation;
            EntityStateMachineComponent = actor;
            ActionTransform = place;
        }

    }
}