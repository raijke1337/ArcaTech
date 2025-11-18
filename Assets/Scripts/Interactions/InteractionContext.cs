using System;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Interactions
{
    [Serializable]
    public class InteractionContext
    {
        public Transform ActionTransform { get; }
        public BaseGameEntityComponent EntityComponent { get; }

        public bool LastInteractionWasSuccessful
        {
            get => success;
            set
            {
                WasUpdated = true;
                success = value;
            }
        }
        private bool success;
        public bool WasUpdated { get; private set; }

        public InteractionContext (BaseGameEntityComponent comp, Transform actionTransform)
        {
            EntityComponent = comp;
            ActionTransform = actionTransform;
            WasUpdated = false;
            success = false;
        }

    }
}