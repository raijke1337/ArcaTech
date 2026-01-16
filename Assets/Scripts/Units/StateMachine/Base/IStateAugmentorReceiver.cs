namespace Arcatech.Units
{
    public interface IStateAugmentorReceiver
    {
        /// <summary>
        /// this needs to be called when the augmentor is not a part of the statemachine gameobject hierarchy
        /// </summary>
        /// <param name="augmentor"></param>
        public void RegisterAugmentor(IStateAugmentor augmentor);
        public void UnregisterAugmentor(IStateAugmentor augmentor);
        public void AddTransition(StateTransition transition);
        public void RemoveTransition(StateTransition transition);
        public StateMachineContext Context { get; }
    }
}