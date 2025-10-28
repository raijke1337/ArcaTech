
namespace Arcatech.Interactions
{
    /// <summary>
    /// this class will handle the logic of the interaction when an interactive item component is triggered and calls the OnInteract()
    /// </summary>
    public interface IInteractionHandler
    {
        public void DoInteraction(IInteractor interactor, IInteractive item);
        public void EndInteraction(IInteractor interactor, IInteractive item);
        
    }
}