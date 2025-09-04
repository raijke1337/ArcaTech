using UnityEngine;

namespace Arcatech.Interactions
{
    public interface IInteractionContext
    {
        public string SomeInformation { get; }
        public ActiveGameUnitComponent ActiveGameUnitComponent { get; }
        public Transform ActionTransform { get; }
    }
}