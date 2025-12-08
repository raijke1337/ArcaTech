namespace Arcatech.Units
{
    public interface IStateAugmentor
    {
        /// <summary>
        /// add transitions here
        /// </summary>
        /// <param name="machine"></param>
        void Attach(EntityStateMachineComponent machine);

        /// Called when the modifier should be removed (weapon unequipped).
        void Detach(EntityStateMachineComponent machine);

        /// Called when the state machine enters a new UnitState.
        void OnStateEntered(UnitState state, StateMachineContext context);

        /// Called when the state machine exits a UnitState.
        void OnStateExited(UnitState state, StateMachineContext context);
    }
}