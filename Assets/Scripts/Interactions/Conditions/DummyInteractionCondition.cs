using UnityEngine;

namespace Arcatech.Interactions
{
    [CreateAssetMenu(fileName = "New dummy condition",menuName = "Interactions/Condition/Dummy")]
    public class DummyInteractionCondition : InteractionCondition
    {
        [SerializeField] private bool _result;
        public override bool CheckCondition(IInteractor actor, IInteractive item, IInteractionContext context)
        {
            return _result; 
        }
    }
}