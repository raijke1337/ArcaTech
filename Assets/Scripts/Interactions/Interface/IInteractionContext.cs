using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Interactions
{
    public interface IInteractionContext
    {
        public string SomeInformation { get; }
        public EntityStateMachineComponent EntityStateMachineComponent { get; }
        public Transform ActionTransform { get; }
    }
}