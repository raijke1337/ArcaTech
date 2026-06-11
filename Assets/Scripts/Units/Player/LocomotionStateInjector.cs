using UnityEngine;

namespace Arcatech.Units.Control
{
    public class LocomotionStateInjector : MonoBehaviour, IStateAugmentor
    {

        [SerializeField] public SerializedStateTransition[] _transitionsToAdd;
        private StateTransition[] _transitions;
        
        public void Attach(IStateAugmentorReceiver machine)
        {
            if (_transitions == null)
            {
                _transitions = new StateTransition[_transitionsToAdd.Length];
                for (int i = 0; i < _transitionsToAdd.Length; i++)
                {
                    _transitions[i] = _transitionsToAdd[i].Build();
                }
            }

            foreach (var t in _transitions)
            {
                machine.AddTransition(t);
            }
        }

        public void Detach(IStateAugmentorReceiver machine)
        {
            foreach (var t in _transitions)
            {
                machine.RemoveTransition(t);
            }
        }

        public void OnStateEntered(UnitState state, StateMachineContext context)
        {
        }

        public void OnStateExited(UnitState state, StateMachineContext context)
        { }

    }
}