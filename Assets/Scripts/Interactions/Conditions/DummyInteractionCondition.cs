using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Interactions
{
   // [CreateAssetMenu(fileName = "New dummy condition",menuName = "Interactions/Condition/Dummy")]
    public class DummyInteractionCondition : InteractionCondition
    {
        [SerializeField] private bool _result;


        public override void Check(InteractionContext ctx, UnityAction<InteractionState> callback)
        {
            callback.Invoke(_result? InteractionState.Success : InteractionState.Failure);
        }
    }
}