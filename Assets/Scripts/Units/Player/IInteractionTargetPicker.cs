namespace Arcatech.Interactions
{
    public interface IInteractionTargetPicker
    {
        public bool HasInteractiveSelected(out IInteractive item);
        public bool DoInteraction(IInteractor interactor);
    }
}