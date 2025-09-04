using UnityEngine;

namespace Arcatech.Interactions
{
    public class InteractionContext : IInteractionContext
    {
        public string SomeInformation { get; }
        public ActiveGameUnitComponent ActiveGameUnitComponent { get; }
        public Transform ActionTransform { get; }
        
        public static InteractionContext Create(ActiveGameUnitComponent actor, Transform place,string text = "undefined context")
        {
            return new InteractionContext(actor,place,text); 
        }

        private InteractionContext(ActiveGameUnitComponent actor, Transform place, string someInformation)
        {
            SomeInformation = someInformation;
            ActiveGameUnitComponent = actor;
            ActionTransform = place;
        }

    }
}