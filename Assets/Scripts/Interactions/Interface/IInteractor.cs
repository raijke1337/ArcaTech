using Arcatech.Units;

namespace Arcatech.Interactions
{
    /// <summary>
    /// some component that will do the interaction
    /// </summary>
    public interface IInteractor
    {
        public InteractionContext InteractionContext { get; }
        public void RegisterInteractiveItemInContext(IInteractive item);
        public void UnregisterInteractiveItemFromContext(IInteractive item);

    }
}