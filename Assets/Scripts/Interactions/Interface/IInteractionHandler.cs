
namespace Arcatech.Interactions
{
    /// <summary>
    /// this class will handle the logic of the interaction when an interactive item component is triggered and calls the OnInteract()
    /// </summary>
    public interface IInteractionHandler
    {
        public void DoInteraction(bool success, IInteractor interactor);
        public void OnPlayerEnter();
        public void OnPlayerExit();

    }
}