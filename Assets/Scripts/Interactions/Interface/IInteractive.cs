namespace Arcatech.Interactions
{
    /// <summary>
    /// the interface for an item that can be interacted with
    /// </summary>

    public interface IInteractive 
    {
        public BaseGameEntityComponent GetBaseComponent { get; }
        public bool TryInteraction(IInteractor interactor);
    }
}