namespace Arcatech.Interactions
{
    /// <summary>
    /// this interface is for checking if interaction can be performed
    /// it uses th strategy pattern
    /// </summary>
    public interface IInteractionCondition
    {
        public bool CheckCondition(IInteractor actor, IInteractive item);
    }



    public abstract class InteractionCondition : ScriptableObjectID, IInteractionCondition
    {
        public abstract bool CheckCondition(IInteractor actor, IInteractive item);
    }
    
    
}