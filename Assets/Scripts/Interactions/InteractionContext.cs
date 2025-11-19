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

        public bool NewInteractionWasPerformed(out bool interactionResult)
        {
            interactionResult = success;
            if (update)
            {
                update = false;
                return true;
            }
            return false;
        }

        public void UpdateInteractionResult(bool result)
        {
            success = result;
            update = true;
        }
        
        private bool success;
        private bool update = false;

        public InteractionContext (BaseGameEntityComponent comp, Transform actionTransform)
        {
            EntityComponent = comp;
            ActionTransform = actionTransform;
            update = false;
            success = false;
        }

    }
}