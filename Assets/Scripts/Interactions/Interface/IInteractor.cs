using Arcatech.Units;

namespace Arcatech.Interactions
{
    /// <summary>
    /// some component that will do the interaction
    /// </summary>
    public interface IInteractor : IPausableComponent
    {
        public InteractionContext InteractionContext { get; }

    }
}